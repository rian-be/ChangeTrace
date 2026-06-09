using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Delegates;
using ChangeTrace.GIt.Helpers;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using ChangeTrace.GIt.Services.Checkpoints.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Main service for exporting repositories to timelines.
/// Orchestrates the complete export pipeline: clone (if needed), read commits, build timeline, enrich with PR data, and persist.
/// This is the central coordinator that ties together all repository export operations.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class RepositoryExporter(
    IGitRepositoryReader gitReader,
    ITimelineBuilder timelineBuilder,
    ITimelineRepository repository,
    ILogger<RepositoryExporter> logger,
    ITimelineEnricherResolver enricherResolver,
    IExportCheckpointStore checkpointStore) : IRepositoryExporter
{
    /// <summary>
    /// Exports repository to timeline.
    /// </summary>
    /// <param name="pathOrUrl">Local repository path or remote Git URL. Remote URLs are automatically cloned to a temporary location.</param>
    /// <param name="options">Options controlling export behavior (date filters, max commits, enrichment, etc.).</param>
    /// <param name="progress">Optional progress callback reporting operation name, current step, total steps, and status message.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Cleans up temporary resources if canceled.</param>
    /// <returns>
    /// A <see cref="Result{Timeline}"/> containing the fully constructed and normalized timeline on success,
    /// or failure with error details (including exception if applicable).
    /// </returns>
    public async Task<Result<Timeline>> ExportAsync(
        string pathOrUrl,
        ExportOptions options,
        ProgressCallback? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting export: {Path}", pathOrUrl);

            // Step 1: Get repository path
            progress?.Invoke("Prepare", 0, 4, "Preparing repository...");
            var pathResult = await GetRepositoryPathStep(pathOrUrl, cancellationToken);
            if (pathResult.IsFailure)
                return Result<Timeline>.Failure(pathResult.Error!);

            await using var checkout = pathResult.Value!;
            var repoPath = checkout.Path;
            progress?.Invoke("Prepare", 1, 4, "Repository ready");

            var repositoryId = ResolveRepositoryId(pathOrUrl, repoPath);
            // Step 2 and 3: Stream commits and build timeline.
            var timelineResult = await BuildTimelineFromRepositoryStep(
                repoPath,
                repositoryId,
                options,
                progress,
                cancellationToken);
            if (timelineResult.IsFailure)
                return Result<Timeline>.Failure(timelineResult.Error!);

            var timeline = timelineResult.Value;
            progress?.Invoke("Build", 3, 4, $"Built {timeline.Count} events");

            // Step 4 and 5 Enrich with PR data & Normalize
            var enrichResult = await EnrichStep(timeline, pathOrUrl, repoPath, options, progress, cancellationToken);
            if (enrichResult.IsFailure)
                return Result<Timeline>.Failure(enrichResult.Error!);

            TimelineNormalizer.Normalize(timeline);

            progress?.Invoke("Complete", 4, 4, "Export complete");

            return Result<Timeline>.Success(timeline);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Export failed");
            return Result<Timeline>.Failure("Export failed", ex);
        }
    }

    /// <summary>
    /// Exports repository to timeline and saves it to file.
    /// </summary>
    /// <param name="pathOrUrl">Local repository path or remote Git URL. Remote URLs are automatically cloned.</param>
    /// <param name="outputPath">Path where the timeline file will be saved (.gittrace format). Directory is created if it doesn't exist.</param>
    /// <param name="options">Options controlling export behavior.</param>
    /// <param name="progress">Optional progress callback reporting progress across export and save operations.</param>
    /// <param name="cancellationToken">Token to cancel the operation. Cancellation during save may leave incomplete file.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success if both export and save completed successfully,
    /// or failure with error details if either operation failed.
    /// </returns>
    public async Task<Result> ExportAndSaveAsync(
        string pathOrUrl,
        string outputPath,
        ExportOptions options,
        ProgressCallback? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (repository is IStreamingTimelineRepository streamingRepository &&
            !options.EnrichmentKinds.HasFlag(ExportEnrichmentKind.PullRequests))
        {
            try
            {
                logger.LogInformation("Starting streaming export+save: {Path}", pathOrUrl);

                progress?.Invoke("Prepare", 0, 4, "Preparing repository...");
                var pathResult = await GetRepositoryPathStep(pathOrUrl, cancellationToken);
                if (pathResult.IsFailure)
                    return Result.Failure(pathResult.Error!);

                await using var checkout = pathResult.Value;
                var repoPath = checkout.Path;
                progress?.Invoke("Prepare", 1, 4, "Repository ready");

                var repositoryId = ResolveRepositoryId(pathOrUrl, repoPath);
                progress?.Invoke("Read", 1, 4, "Reading commits...");

                var backend = SelectHistoryBackend(repoPath, options);
                var commitsResult = await gitReader.ReadCommitsStreamAsync(
                    repoPath,
                    new GitReaderOptions(
                        IncludeFileChanges: options.IncludeFileChanges,
                        MaxCommits: options.MaxCommits,
                        StartDate: options.StartDate,
                        EndDate: options.EndDate,
                        Backend: backend,
                        DetectRenames: options.DetectRenames,
                        IncludeBranches: options.IncludeBranchEvents || options.IncludeMergeDetection
                    ),
                    cancellationToken);

                if (commitsResult.IsFailure)
                    return Result.Failure(commitsResult.Error!);

                progress?.Invoke("Build", 2, 4, "Streaming timeline to repository...");

                var streamSaveResult = await streamingRepository.SaveAsync(
                    commitsResult.Value,
                    outputPath,
                    new TimelineBuilderOptions(
                        IncludeFileChanges: options.IncludeFileChanges,
                        IncludeBranchEvents: options.IncludeBranchEvents,
                        IncludeMergeDetection: options.IncludeMergeDetection,
                        RepositoryId: repositoryId),
                    cancellationToken);

                if (streamSaveResult.IsFailure)
                    return Result.Failure(streamSaveResult.Error!);

                progress?.Invoke("Complete", 4, 4, "Export complete");
                logger.LogInformation("Timeline saved to: {Path}", outputPath);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Streaming export+save failed");
                return Result.Failure("Export failed", ex);
            }
        }

        var checkpointOptions = options with
        {
            CheckpointKey = outputPath,
            CheckpointFingerprint = CreateCheckpointFingerprint(pathOrUrl, options)
        };

        try
        {
            logger.LogInformation("Starting resumable export+save: {Path}", pathOrUrl);

            progress?.Invoke("Prepare", 0, 4, "Preparing repository...");

            Timeline? timeline;
            var checkpoint = await LoadCheckpointAsync(checkpointOptions, cancellationToken);

            if (checkpoint is not null)
            {
                logger.LogInformation(
                    "Resuming export from checkpoint: {Stage}",
                    checkpoint.Stage);

                timeline = checkpoint.Timeline;
            }
            else
            {
                var pathResult = await GetRepositoryPathStep(pathOrUrl, cancellationToken);
                if (pathResult.IsFailure)
                    return Result.Failure(pathResult.Error!);

                await using var checkout = pathResult.Value;
                var repoPath = checkout.Path;
                progress?.Invoke("Prepare", 1, 4, "Repository ready");

                var repositoryId = ResolveRepositoryId(pathOrUrl, repoPath);
                var timelineResult = await BuildTimelineFromRepositoryStep(
                    repoPath,
                    repositoryId,
                    checkpointOptions,
                    progress,
                    cancellationToken);
                if (timelineResult.IsFailure)
                    return Result.Failure(timelineResult.Error!);

                timeline = timelineResult.Value;
                progress?.Invoke("Build", 3, 4, $"Built {timeline.Count} events");

                await SaveCheckpointAsync(
                    checkpointOptions,
                    timeline,
                    ExportCheckpointStage.Built,
                    1,
                    0,
                    cancellationToken);
            }

            if (ShouldRunEnrichment(checkpoint))
            {
                var enrichResult = await EnrichStep(
                    timeline,
                    pathOrUrl,
                    pathOrUrl,
                    checkpointOptions,
                    progress,
                    cancellationToken);

                if (enrichResult.IsFailure)
                    return Result.Failure(enrichResult.Error!);

                await SaveCheckpointAsync(
                    checkpointOptions,
                    timeline,
                    ExportCheckpointStage.SavingTimeline,
                    0,
                    0,
                    cancellationToken);
            }

            TimelineNormalizer.Normalize(timeline);

            var saveResult = await repository.SaveAsync(
                timeline,
                outputPath,
                cancellationToken);

            if (saveResult.IsFailure)
                return Result.Failure(saveResult.Error!);

            await ClearCheckpointAsync(checkpointOptions, cancellationToken);

            logger.LogInformation("Timeline saved to: {Path}", outputPath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resumable export+save failed");
            return Result.Failure("Export failed", ex);
        }
    }

    /// <summary>
    /// Gets local repository path, cloning if URL is provided.
    /// </summary>
    /// <param name="pathOrUrl">Local path or remote URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{String}"/> containing the local repository path.
    /// For URLs, returns path to temporary clone; for local paths, returns the path if it exists.
    /// </returns>
    private async Task<Result<RepositoryCheckout>> GetRepositoryPathStep(
        string pathOrUrl,
        CancellationToken cancellationToken)
    {
        if (IsGitUrl(pathOrUrl))
        {
            logger.LogInformation("Cloning repository: {Url}", pathOrUrl);

            var tempPath = Path.Combine(Path.GetTempPath(), "ChangeTrace", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempPath);

            var cloneResult = await gitReader.CloneAsync(pathOrUrl, tempPath, cancellationToken);
            return cloneResult.IsFailure
                ? Result<RepositoryCheckout>.Failure(cloneResult.Error!)
                : Result<RepositoryCheckout>.Success(new RepositoryCheckout(tempPath, tempPath));
        }

        return !Directory.Exists(pathOrUrl)
            ? Result<RepositoryCheckout>.Failure($"Directory not found: {pathOrUrl}")
            : Result<RepositoryCheckout>.Success(new RepositoryCheckout(pathOrUrl, null));
    }

    private async Task<Result<Timeline>> BuildTimelineFromRepositoryStep(
        string repoPath,
        RepositoryId? repositoryId,
        ExportOptions options,
        ProgressCallback? progress,
        CancellationToken ct)
    {
        progress?.Invoke("Read", 1, 4, "Reading commits...");

        var backend = SelectHistoryBackend(repoPath, options);

        var commitsResult = await gitReader.ReadCommitsStreamAsync(
            repoPath,
            new GitReaderOptions(
                IncludeFileChanges: options.IncludeFileChanges,
                MaxCommits: options.MaxCommits,
                StartDate: options.StartDate,
                EndDate: options.EndDate,
                Backend: backend,
                DetectRenames: options.DetectRenames,
                IncludeBranches: options.IncludeBranchEvents || options.IncludeMergeDetection
            ),
            ct
        );

        if (commitsResult.IsFailure)
            return Result<Timeline>.Failure(commitsResult.Error!);

        progress?.Invoke("Build", 2, 4, "Building timeline...");

        var builderOptions = new TimelineBuilderOptions(
            IncludeFileChanges: options.IncludeFileChanges,
            IncludeBranchEvents: options.IncludeBranchEvents,
            IncludeMergeDetection: options.IncludeMergeDetection,
            RepositoryId: repositoryId
        );

        return await timelineBuilder.Build(commitsResult.Value, builderOptions, ct);
    }

    private GitHistoryReaderBackend SelectHistoryBackend(
        string repoPath,
        ExportOptions options)
    {
        if (options.HistoryBackend != GitHistoryReaderBackend.LibGit2Sharp)
            return options.HistoryBackend;

        if (!IsGitCliAvailable())
            return GitHistoryReaderBackend.LibGit2Sharp;

        if (options.IncludeFileChanges)
        {
            logger.LogInformation(
                "IncludeFileChanges enabled. Automatically switching to Git CLI history backend for file change extraction throughput.");
            return GitHistoryReaderBackend.GitCli;
        }

        var commitCount = GetCommitCount(repoPath);
        if (commitCount > 20000)
        {
            logger.LogWarning(
                "Large repository detected ({Count} commits). Automatically switching to Git CLI streaming backend for performance.",
                commitCount);
            return GitHistoryReaderBackend.GitCli;
        }

        return GitHistoryReaderBackend.LibGit2Sharp;
    }

    /// <summary>
    /// Enriches timeline with pull request data using <see cref="ITimelineEnricher"/>.
    /// </summary>
    /// <param name="timeline">Timeline to enrich.</param>
    /// <param name="pathOrUrl">Repository path or URL (used to derive repository ID).</param>
    /// <param name="options">Export options controlling enrichment.</param>
    /// <param name="progress">Optional progress callback.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task completing when enrichment finishes.</returns>
    private async Task<Result> EnrichStep(
        Timeline timeline,
        string pathOrUrl,
        string repoPath,
        ExportOptions options,
        ProgressCallback? progress,
        CancellationToken ct)
    {
        if (!options.EnrichmentKinds.HasFlag(ExportEnrichmentKind.PullRequests))
            return Result.Success();

        var providerResult = TryDetectProvider(pathOrUrl, repoPath, options);
        if (providerResult.IsFailure)
            return Result.Failure(providerResult.Error!);

        var provider = providerResult.Value;
        if (provider is null)
        {
            logger.LogInformation("No hosting provider detected for enrichment; skipping PR enrichment.");
            return Result.Success();
        }

        if (!enricherResolver.TryResolve(provider, out var enricher) || enricher is null)
        {
            logger.LogWarning("No timeline enricher registered for provider '{Provider}'", provider);
            return Result.Success();
        }

        var repositoryId = ResolveRepositoryId(pathOrUrl, repoPath);
        if (repositoryId == null)
            return Result.Failure($"Unable to derive repository identifier for provider '{provider}'.");

        progress?.Invoke("Enrich", 3, 4, "Enriching with PR data...");
        var enrichResult = await enricher.Enrich(timeline, repositoryId, options, ct);

        if (enrichResult.IsSuccess)
            logger.LogInformation("Enriched {Count} events", enrichResult.Value.MatchedCount);
        else
            logger.LogWarning("PR enrichment failed: {Error}", enrichResult.Error);

        return enrichResult.IsSuccess
            ? Result.Success()
            : Result.Failure(enrichResult.Error!);
    }

    private static Result<string?> TryDetectProvider(string pathOrUrl, string repoPath, ExportOptions options)
    {
        var remoteUrl = GetRemoteOriginUrl(repoPath);
        var providerSource = remoteUrl ?? pathOrUrl;

        if (!IsGitUrl(providerSource))
            return Result<string?>.Success(null);

        try
        {
            return Result<string?>.Success(ProviderUrlHelper.DetectProvider(providerSource));
        }
        catch (NotSupportedException ex)
        {
            if (IsGitLabRepository(providerSource, options.GitLabBaseUrl))
                return Result<string?>.Success("gitlab");

            return Result<string?>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<string?>.Failure($"Unable to detect repository provider: {ex.Message}", ex);
        }
    }

    private static bool IsGitLabRepository(string pathOrUrl, string gitLabBaseUrl)
    {
        var expectedHost = TryExtractHost(gitLabBaseUrl);
        var actualHost = TryExtractHost(pathOrUrl);

        return expectedHost is not null
               && actualHost is not null
               && string.Equals(expectedHost, actualHost, StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryExtractHost(string repository)
    {
        if (Uri.TryCreate(repository, UriKind.Absolute, out var uri))
            return uri.Host;

        var atIndex = repository.IndexOf('@');
        var colonIndex = repository.IndexOf(':');

        if (atIndex < 0 || colonIndex <= atIndex)
            return null;

        return repository.Substring(atIndex + 1, colonIndex - atIndex - 1);
    }

    /// <summary>
    /// Determines if a string is a Git URL.
    /// </summary>
    /// <param name="pathOrUrl">String to check.</param>
    /// <returns>True if string starts with http://, https://, or git@; otherwise false.</returns>
    private static bool IsGitUrl(string pathOrUrl) =>
         pathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
               pathOrUrl.StartsWith("git@", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Repository path scope with optional temporary checkout cleanup.
    /// </summary>
    private sealed class RepositoryCheckout(string path, string? temporaryPath) : IAsyncDisposable
    {
        /// <summary>
        /// Gets the resolved repository path.
        /// </summary>
        public string Path { get; } = path;

        /// <summary>
        /// Disposes the temporary checkout if one was created.
        /// </summary>
        public ValueTask DisposeAsync()
        {
            if (string.IsNullOrWhiteSpace(temporaryPath))
                return ValueTask.CompletedTask;

            try
            {
                if (Directory.Exists(temporaryPath))
                    Directory.Delete(temporaryPath, true);
            }
            catch
            {
                // Best effort cleanup.
            }

            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Extracts repository owner and name from Git URL.
    /// </summary>
    /// <param name="pathOrUrl">Git URL to parse.</param>
    /// <returns>
    /// <see cref="RepositoryId"/> if URL can be parsed (e.g., github.com/owner/name), otherwise null.
    /// Handles both HTTPS and SSH formats, stripping .git extension.
    /// </returns>
    private static RepositoryId? ExtractRepositoryId(string pathOrUrl)
    {
        if (!IsGitUrl(pathOrUrl))
            return null;

        try
        {
            var url = pathOrUrl;

            if (pathOrUrl.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
            {
                var atIndex = pathOrUrl.IndexOf('@');
                var colonIndex = pathOrUrl.IndexOf(':');

                if (atIndex < 0 || colonIndex <= atIndex)
                    return null;

                var host = pathOrUrl.Substring(atIndex + 1, colonIndex - atIndex - 1);
                var remainder = pathOrUrl[(colonIndex + 1)..];
                url = $"https://{host}/{remainder}";
            }

            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Trim('/').Split('/');

            if (segments.Length >= 2)
            {
                var owner = segments[0];
                var name = segments[1].Replace(".git", "");
                return RepositoryId.Create(owner, name).ValueOrNull;
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return null;
    }

    private static RepositoryId? ResolveRepositoryId(
        string pathOrUrl,
        string repoPath)
    {
        var direct = ExtractRepositoryId(pathOrUrl);
        if (direct is not null)
            return direct;

        var remoteUrl = GetRemoteOriginUrl(repoPath);
        return remoteUrl is null
            ? null
            : ExtractRepositoryId(remoteUrl);
    }

    private async Task<ExportCheckpointState?> LoadCheckpointAsync(
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.CheckpointKey) ||
            string.IsNullOrWhiteSpace(options.CheckpointFingerprint))
        {
            return null;
        }

        return await checkpointStore.TryLoad(
            options.CheckpointKey,
            options.CheckpointFingerprint,
            cancellationToken);
    }

    private async Task SaveCheckpointAsync(
        ExportOptions options,
        Timeline timeline,
        ExportCheckpointStage stage,
        int nextPullRequestPage,
        int nextPullRequestIndex,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.CheckpointKey) ||
            string.IsNullOrWhiteSpace(options.CheckpointFingerprint))
        {
            return;
        }

        try
        {
            await checkpointStore.Save(
                options.CheckpointKey,
                new ExportCheckpointState(
                    options.CheckpointFingerprint,
                    stage,
                    nextPullRequestPage,
                    nextPullRequestIndex,
                    timeline),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist export checkpoint for {Stage}; continuing without resume data.", stage);
        }
    }

    private async Task ClearCheckpointAsync(
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.CheckpointKey) ||
            string.IsNullOrWhiteSpace(options.CheckpointFingerprint))
        {
            return;
        }

        try
        {
            await checkpointStore.Clear(options.CheckpointKey, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to clear export checkpoint; leaving stale resume data in place.");
        }
    }

    private static bool ShouldRunEnrichment(ExportCheckpointState? checkpoint)
        => checkpoint is null
           || checkpoint.Stage is ExportCheckpointStage.Built
           || checkpoint.Stage is ExportCheckpointStage.EnrichingPullRequests;

    private static string CreateCheckpointFingerprint(
        string pathOrUrl,
        ExportOptions options)
    {
        var normalized = string.Join(
            "|",
            pathOrUrl.Trim(),
            options.IncludeFileChanges,
            options.IncludeBranchEvents,
            options.IncludeMergeDetection,
            options.EnrichmentKinds,
            options.HistoryBackend,
            options.DetectRenames,
            options.StartDate?.ToUnixTimeSeconds() ?? -1,
            options.EndDate?.ToUnixTimeSeconds() ?? -1,
            options.GitLabBaseUrl.Trim());

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }

    private static string? GetRemoteOriginUrl(string repoPath)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add("-C");
            process.StartInfo.ArgumentList.Add(repoPath);
            process.StartInfo.ArgumentList.Add("remote");
            process.StartInfo.ArgumentList.Add("get-url");
            process.StartInfo.ArgumentList.Add("origin");

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return null;

            var remoteUrl = output.Trim();
            return string.IsNullOrWhiteSpace(remoteUrl) ? null : remoteUrl;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsGitCliAvailable()
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo.ArgumentList.Add("--version");
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static int GetCommitCount(string repoPath)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.StartInfo.ArgumentList.Add("-C");
            process.StartInfo.ArgumentList.Add(repoPath);
            process.StartInfo.ArgumentList.Add("rev-list");
            process.StartInfo.ArgumentList.Add("--count");
            process.StartInfo.ArgumentList.Add("HEAD");

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0 && int.TryParse(output.Trim(), out var count))
                return count;
        }
        catch
        {
            // Ignore
        }

        return 0;
    }
}
