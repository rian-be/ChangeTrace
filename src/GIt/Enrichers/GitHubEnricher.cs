using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Options;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Services;
using ChangeTrace.GIt.Services.Checkpoints.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;

namespace ChangeTrace.GIt.Enrichers;

/// <summary>
/// GitHub specific timeline enricher using <see cref="BasePlatformEnricher"/>.
/// Pragmatic: direct, no over-abstraction.
/// </summary>
/// <remarks>
/// Enriches a <see cref="Timeline"/> with pull request events fetched from GitHub.
/// Matches PRs against commits or branches in the timeline and attaches PR metadata.
/// Handles pagination, API rate limits, and errors gracefully.
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton, typeof(IProviderTimelineEnricher))]
internal sealed class GitHubEnricher(
    ILogger<GitHubEnricher> logger,
    IExportCheckpointStore checkpointStore)
    : BasePlatformEnricher(logger), IProviderTimelineEnricher
{
    /// <summary>
    /// Provider handled by this enricher.
    /// </summary>
    public string Provider => "github";

    /// <summary>
    /// Enriches the timeline with GitHub pull requests.
    /// </summary>
    /// <param name="timeline">Timeline to enrich</param>
    /// <param name="repositoryId">Repository identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of PRs processed, matched, and unmatched in <see cref="EnrichmentResult"/></returns>
    public override async Task<Result<EnrichmentResult>> Enrich(
        Timeline timeline,
        RepositoryId repositoryId,
        ExportOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Fetching PRs from {Repo}", repositoryId.FullName);

            if (string.IsNullOrWhiteSpace(options.GitHubToken))
            {
                Logger.LogWarning(
                    "GitHub enrichment is running without a token. Anonymous requests have a much lower rate limit.");
            }

            var checkpoint = await LoadCheckpointAsync(options, cancellationToken);
            var resumePage = checkpoint?.Stage == ExportCheckpointStage.EnrichingPullRequests
                ? Math.Max(1, checkpoint.NextPullRequestPage)
                : 1;
            var resumeIndex = checkpoint?.Stage == ExportCheckpointStage.EnrichingPullRequests
                ? Math.Max(0, checkpoint.NextPullRequestIndex)
                : 0;

            if (checkpoint is not null &&
                checkpoint.Stage is ExportCheckpointStage.Built or ExportCheckpointStage.EnrichingPullRequests)
            {
                timeline.Clear();
                timeline.AddEvents(checkpoint.Timeline.Events);
            }

            var client = CreateClient(options);

            int matched = 0;
            int total = 0;
            var wasRateLimited = false;

            for (var page = resumePage; !cancellationToken.IsCancellationRequested; page++)
            {
                IReadOnlyList<PullRequest> prs;

                try
                {
                    prs = await client.PullRequest.GetAllForRepository(
                        repositoryId.Owner,
                        repositoryId.Name,
                        new PullRequestRequest { State = ItemStateFilter.All },
                        new ApiOptions { PageCount = 1, PageSize = 100, StartPage = page });
                }
                catch (RateLimitExceededException ex)
                {
                    wasRateLimited = true;
                    Logger.LogWarning(ex, "GitHub rate limit exceeded while fetching page {Page}", page);
                    break;
                }

                if (!prs.Any())
                    break;

                total += prs.Count;

                for (var index = page == resumePage ? resumeIndex : 0; index < prs.Count; index++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return Result<EnrichmentResult>.Failure("Cancelled");

                    var pr = prs[index];
                    var targetIndex = FindMatchingEventIndex(timeline, pr);
                    if (targetIndex is null)
                        continue;

                    var prNumber = PullRequestNumber.Create(pr.Number).Value;
                    var prType = MapPrState(pr.Merged, pr.State.StringValue);
                    var metadata = $"PR#{pr.Number} by {pr.User.Login} -> {pr.Base.Ref}";

                    var updated = timeline.TryUpdateAt(
                        targetIndex.Value,
                        evt => EnrichTraceEventWithPr(evt, prNumber, prType, metadata));

                    if (!updated)
                        continue;

                    matched++;

                    await SavePatchAsync(
                        options,
                        timeline,
                        page,
                        index + 1,
                        targetIndex.Value,
                        timeline.Events[targetIndex.Value],
                        cancellationToken);
                }

                resumeIndex = 0;
                await SaveSnapshotAsync(
                    options,
                    timeline,
                    ExportCheckpointStage.EnrichingPullRequests,
                    page + 1,
                    0,
                    cancellationToken);
            }

            if (total == 0)
            {
                if (wasRateLimited)
                    Logger.LogWarning("GitHub rate limit reached before any PRs could be fetched; skipping PR enrichment.");
                else
                    Logger.LogWarning("No PRs found");

                await SaveSnapshotAsync(
                    options,
                    timeline,
                    ExportCheckpointStage.Enriched,
                    0,
                    0,
                    cancellationToken);

                return Result<EnrichmentResult>.Success(new EnrichmentResult(0, 0, 0));
            }

            if (wasRateLimited)
            {
                Logger.LogWarning(
                    "GitHub rate limit reached after fetching {Count} pull requests; continuing with partial enrichment.",
                    total);
            }

            await SaveSnapshotAsync(
                options,
                timeline,
                ExportCheckpointStage.Enriched,
                0,
                0,
                cancellationToken);

            Logger.LogInformation("Enrichment complete: {Matched}/{Total} matched", matched, total);
            return Result<EnrichmentResult>.Success(new EnrichmentResult(total, matched, total - matched));
        }
        catch (NotFoundException ex)
        {
            Logger.LogError(ex, "Repository not found");
            return Result<EnrichmentResult>.Failure("Repository not found", ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "GitHub enrichment failed");
            return Result<EnrichmentResult>.Failure("Enrichment failed", ex);
        }
    }

    /// <summary>
    /// Creates the GitHub client.
    /// </summary>
    private static GitHubClient CreateClient(ExportOptions options)
    {
        var client = new GitHubClient(new ProductHeaderValue("ChangeTrace"));

        if (!string.IsNullOrWhiteSpace(options.GitHubToken))
            client.Credentials = new Credentials(options.GitHubToken);

        return client;
    }

    /// <summary>
    /// Attempts to find the timeline event index that matches the given PR via merge SHA, head SHA, or branch name.
    /// </summary>
    private int? FindMatchingEventIndex(Timeline timeline, PullRequest pr)
    {
        // 1. Merge commit SHA
        if (!string.IsNullOrEmpty(pr.MergeCommitSha))
        {
            var shaResult = CommitSha.Create(pr.MergeCommitSha);
            if (shaResult.IsSuccess)
            {
                var match = FindFirstIndex(timeline, e => e.Commit?.Sha != null && e.Commit.Value.Sha.Matches(shaResult.Value));
                if (match is not null)
                {
                    Logger.LogDebug("PR #{Number} matched via merge SHA", pr.Number);
                    return match.Value.Index;
                }
            }
        }

        // 2. Head SHA
        if (pr.Head?.Sha != null)
        {
            var shaResult = CommitSha.Create(pr.Head.Sha);
            if (shaResult.IsSuccess)
            {
                var match = FindFirstIndex(timeline, e => e.Commit?.Sha != null && e.Commit.Value.Sha.Matches(shaResult.Value));
                if (match is not null)
                {
                    Logger.LogDebug("PR #{Number} matched via head SHA", pr.Number);
                    return match.Value.Index;
                }
            }
        }

        // 3. Branch name
        if (!string.IsNullOrEmpty(pr.Head?.Ref))
        {
            var branchResult = BranchName.Create(pr.Head.Ref);
            if (branchResult.IsSuccess)
            {
                var match = FindFirstIndex(timeline, e => e.Branch?.Name != null && e.Branch.Value.Name == branchResult.Value);
                if (match is not null)
                {
                    Logger.LogDebug("PR #{Number} matched via branch", pr.Number);
                    return match.Value.Index;
                }
            }
        }

        return null;
    }

    private static (int Index, TraceEvent Event)? FindFirstIndex(Timeline timeline, Func<TraceEvent, bool> predicate)
    {
        var events = timeline.EventsSpan;
        for (var index = 0; index < events.Length; index++)
        {
            var evt = events[index];
            if (predicate(evt))
                return (index, evt);
        }

        return null;
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

    private async Task SaveSnapshotAsync(
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
            Logger.LogWarning(ex, "Failed to persist PR enrichment checkpoint snapshot for {Stage}; continuing.", stage);
        }
    }

    private async Task SavePatchAsync(
        ExportOptions options,
        Timeline timeline,
        int nextPullRequestPage,
        int nextPullRequestIndex,
        int targetIndex,
        TraceEvent updatedEvent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.CheckpointKey) ||
            string.IsNullOrWhiteSpace(options.CheckpointFingerprint))
        {
            return;
        }

        try
        {
            await checkpointStore.AppendPullRequestPatch(
                options.CheckpointKey,
                new ExportCheckpointState(
                    options.CheckpointFingerprint,
                    ExportCheckpointStage.EnrichingPullRequests,
                    nextPullRequestPage,
                    nextPullRequestIndex,
                    timeline),
                targetIndex,
                updatedEvent,
                cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to persist PR enrichment patch; continuing.");
        }
    }
}
