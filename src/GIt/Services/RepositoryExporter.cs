using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Delegates;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
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
    ITimelineEnricher? githubEnricher = null) : IRepositoryExporter
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

            var repoPath = pathResult.Value;
            progress?.Invoke("Prepare", 1, 4, "Repository ready");

            // Step 2: Read commits
            var commitsResult = await ReadCommitsStep(repoPath, options, progress, cancellationToken);
            if (commitsResult.IsFailure)
                return Result<Timeline>.Failure(commitsResult.Error!);

            var commits = commitsResult.Value;
            
            // Step 3: Build timeline
            var timelineResult = BuildTimelineStep(commits, options, progress);
            if (timelineResult.IsFailure)
                return Result<Timeline>.Failure(timelineResult.Error!);
            
            var timeline = timelineResult.Value;
            progress?.Invoke("Build", 3, 4, $"Built {timeline.Count} events");

            // Step 4 and 5 Enrich with PR data & Normalize
            await EnrichStep(timeline, pathOrUrl, options, progress, cancellationToken);
            timeline.Normalize(); 
            
            progress?.Invoke("Complete", 4, 4, "Export complete");
            var stats = timeline.GetStatistics();
            logger.LogInformation("Export complete: {Stats}", stats);

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
        var exportResult = await ExportAsync(pathOrUrl, options, progress, cancellationToken);
        
        if (exportResult.IsFailure)
            return Result.Failure(exportResult.Error!);

        var saveResult = await repository.SaveAsync(
            exportResult.Value,
            outputPath,
            cancellationToken
        );

        if (saveResult.IsFailure)
            return Result.Failure(saveResult.Error!);

        logger.LogInformation("Timeline saved to: {Path}", outputPath);
        return Result.Success();
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
    private async Task<Result<string>> GetRepositoryPathStep(
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
                ? Result<string>.Failure(cloneResult.Error!)
                : Result<string>.Success(tempPath);
        }

        return !Directory.Exists(pathOrUrl)
            ? Result<string>.Failure($"Directory not found: {pathOrUrl}")
            : Result<string>.Success(pathOrUrl);
    }

    /// <summary>
    /// Reads commits from  repository path using <see cref="IGitRepositoryReader"/>.
    /// </summary>
    /// <param name="repoPath">Local repository path.</param>
    /// <param name="options">Export options controlling commit selection.</param>
    /// <param name="progress">Optional progress callback to report read progress.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task<Result<IReadOnlyList<CommitData>>> ReadCommitsStep(
        string repoPath,
        ExportOptions options,
        ProgressCallback? progress,
        CancellationToken ct)
    {
        progress?.Invoke("Read", 1, 4, "Reading commits...");

        var commitsResult = await gitReader.ReadCommitsAsync(
            repoPath,
            new GitReaderOptions(
                IncludeFileChanges: options.IncludeFileChanges,
                MaxCommits: options.MaxCommits,
                StartDate: options.StartDate,
                EndDate: options.EndDate
            ),
            ct
        );

        if (commitsResult.IsFailure) return commitsResult;

        var commits = commitsResult.Value;
        progress?.Invoke("Read", 2, 4, $"Read {commits.Count} commits");

        return Result<IReadOnlyList<CommitData>>.Success(commits);
    }
    
    /// <summary>
    /// Builds timeline from list of commits using <see cref="ITimelineBuilder"/>.
    /// </summary>
    /// <param name="commits">List of commits to build timeline from.</param>
    /// <param name="options">Export options influencing timeline construction.</param>
    /// <param name="progress">Optional progress callback.</param>
    /// <returns>
    /// A <see cref="Result{Timeline}"/> containing the built timeline or error.
    /// </returns>
    private Result<Timeline> BuildTimelineStep(
        IReadOnlyList<CommitData> commits, 
        ExportOptions options,
        ProgressCallback? progress)
    {
        progress?.Invoke("Build", 2, 4, "Building timeline...");
        var repositoryId = ExtractRepositoryId(options.TimelineName ?? string.Empty);

        var builderOptions = new TimelineBuilderOptions(
            IncludeFileChanges: options.IncludeFileChanges,
            IncludeBranchEvents: options.IncludeBranchEvents,
            IncludeMergeDetection: options.IncludeMergeDetection,
            Name: options.TimelineName,
            RepositoryId: repositoryId
        );

        return timelineBuilder.Build(commits, builderOptions);
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
    private async Task EnrichStep(
        Timeline timeline,
        string pathOrUrl,
        ExportOptions options,
        ProgressCallback? progress,
        CancellationToken ct)
    {
        if (!options.EnrichWithPullRequests || githubEnricher == null)
            return;

        var repositoryId = ExtractRepositoryId(pathOrUrl);
        if (repositoryId == null) return;

        progress?.Invoke("Enrich", 3, 4, "Enriching with PR data...");
        var enrichResult = await githubEnricher.EnrichAsync(timeline, repositoryId, ct);

        if (enrichResult.IsSuccess)
            logger.LogInformation("Enriched {Count} events", enrichResult.Value.MatchedCount);
        else
            logger.LogWarning("PR enrichment failed: {Error}", enrichResult.Error);
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
            var url = pathOrUrl.Replace("git@github.com:", "https://github.com/");
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
}
