using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Models;
using ChangeTrace.GIt.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NGitLab;
using NGitLab.Models;

namespace ChangeTrace.GIt.Enrichers;

/// <summary>
/// GitLab specific timeline enricher.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IProviderTimelineEnricher))]
internal class GitLabEnricher(
    IOptions<ExportOptions> options,
    ILogger<GitLabEnricher> logger) : BasePlatformEnricher(logger), IProviderTimelineEnricher
{
    private readonly GitLabClient _client = CreateClient(options.Value);

    /// <summary>
    /// Provider handled by this enricher.
    /// </summary>
    public string Provider => "gitlab";

    /// <summary>
    /// Enriches the timeline with GitLab merge request data.
    /// </summary>
    public override Task<Result<EnrichmentResult>> EnrichAsync(
        Timeline timeline,
        RepositoryId repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Fetching merge requests from {Repo}", repositoryId.FullName);

            var lookup = BuildLookup(timeline);
            var processed = 0;
            var matched = 0;

            foreach (var mergeRequest in GetMergeRequests(repositoryId, cancellationToken))
            {
                processed++;

                if (!TryResolveMatchIndex(mergeRequest, lookup, out var matchIndex))
                    continue;

                var prNumber = PullRequestNumber.FromTrustedSerialized(mergeRequest.Iid);
                var prType = MapPrState(IsMerged(mergeRequest), mergeRequest.State ?? "opened");
                var metadata = BuildMetadata(mergeRequest);

                if (timeline.TryUpdateAt(
                        matchIndex,
                        evt => EnrichTraceEventWithPr(evt, prNumber.Value, prType, metadata)))
                {
                    matched++;
                }
            }

            Logger.LogInformation("Enrichment complete: {Matched}/{Total} matched", matched, processed);
            return Task.FromResult(Result<EnrichmentResult>.Success(new EnrichmentResult(processed, matched, processed - matched)));
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(Result<EnrichmentResult>.Failure("Cancelled"));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "GitLab enrichment failed");
            return Task.FromResult(Result<EnrichmentResult>.Failure("GitLab enrichment failed", ex));
        }
    }

    /// <summary>
    /// Returns merge requests for a repository.
    /// </summary>
    protected virtual IEnumerable<GitLabMergeRequestSnapshot> GetMergeRequests(
        RepositoryId repositoryId,
        CancellationToken cancellationToken = default)
    {
        var projectId = new ProjectId(repositoryId.FullName);
        var mrClient = _client.GetMergeRequest(projectId);

        foreach (var mergeRequest in mrClient.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return new GitLabMergeRequestSnapshot(
                checked((int)mergeRequest.Iid),
                mergeRequest.MergeCommitSha,
                mergeRequest.Sha,
                mergeRequest.SourceBranch,
                mergeRequest.TargetBranch,
                mergeRequest.State.ToString(),
                mergeRequest.MergedAt,
                mergeRequest.Author?.Username);
        }
    }

    /// <summary>
    /// Builds a timeline lookup.
    /// </summary>
    private static TimelineLookup BuildLookup(Timeline timeline)
    {
        var commitIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var branchIndices = new Dictionary<string, int>(StringComparer.Ordinal);
        var events = timeline.EventsSpan;

        for (var index = 0; index < events.Length; index++)
        {
            var evt = events[index];

            if (evt.Commit?.Sha is { } sha)
                commitIndices.TryAdd(sha.Value, index);

            if (evt.Branch?.Name is { } branch)
                branchIndices.TryAdd(branch.Value, index);
        }

        return new TimelineLookup(commitIndices, branchIndices);
    }

    /// <summary>
    /// Resolves a matching event index.
    /// </summary>
    private static bool TryResolveMatchIndex(
        GitLabMergeRequestSnapshot mergeRequest,
        TimelineLookup lookup,
        out int index)
    {
        if (TryResolveCommitIndex(mergeRequest.MergeCommitSha, lookup.CommitIndices, out index))
            return true;

        if (TryResolveCommitIndex(mergeRequest.Sha, lookup.CommitIndices, out index))
            return true;

        if (TryResolveBranchIndex(mergeRequest.SourceBranch, lookup.BranchIndices, out index))
            return true;

        index = -1;
        return false;
    }

    /// <summary>
    /// Resolves a commit index.
    /// </summary>
    private static bool TryResolveCommitIndex(
        string? value,
        IReadOnlyDictionary<string, int> indices,
        out int index)
    {
        index = -1;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var shaResult = CommitSha.Create(value);
        return shaResult.IsSuccess && indices.TryGetValue(shaResult.Value.Value, out index);
    }

    /// <summary>
    /// Resolves a branch index.
    /// </summary>
    private static bool TryResolveBranchIndex(
        string? value,
        IReadOnlyDictionary<string, int> indices,
        out int index)
    {
        index = -1;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var branchResult = BranchName.Create(value);
        return branchResult.IsSuccess && indices.TryGetValue(branchResult.Value.Value, out index);
    }

    /// <summary>
    /// Checks whether a merge request was merged.
    /// </summary>
    private static bool IsMerged(GitLabMergeRequestSnapshot mergeRequest)
        => string.Equals(mergeRequest.State, "merged", StringComparison.OrdinalIgnoreCase)
           || mergeRequest.MergedAt.HasValue;

    /// <summary>
    /// Builds enrichment metadata.
    /// </summary>
    private static string BuildMetadata(GitLabMergeRequestSnapshot mergeRequest)
    {
        var author = mergeRequest.AuthorUsername ?? "unknown";
        var targetBranch = mergeRequest.TargetBranch ?? "unknown";
        return $"MR!{mergeRequest.Iid} by {author} -> {targetBranch}";
    }

    /// <summary>
    /// Timeline lookup for GitLab enrichment.
    /// </summary>
    private readonly record struct TimelineLookup(
        IReadOnlyDictionary<string, int> CommitIndices,
        IReadOnlyDictionary<string, int> BranchIndices);

    /// <summary>
    /// Creates the GitLab client.
    /// </summary>
    private static GitLabClient CreateClient(ExportOptions options)
        => string.IsNullOrWhiteSpace(options.GitLabToken)
            ? new GitLabClient(options.GitLabBaseUrl)
            : new GitLabClient(options.GitLabBaseUrl, options.GitLabToken);
}
