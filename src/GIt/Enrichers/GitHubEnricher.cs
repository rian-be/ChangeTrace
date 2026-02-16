using ChangeTrace.Core;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
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
internal sealed class GitHubEnricher : BasePlatformEnricher
{
    private readonly GitHubClient _client;
    
    internal GitHubEnricher(string? githubToken, ILogger<GitHubEnricher> logger)
        : base(logger)
    {
        _client = new GitHubClient(new ProductHeaderValue("ChangeTrace"));

        if (!string.IsNullOrEmpty(githubToken))
            _client.Credentials = new Credentials(githubToken);
        else
            Logger.LogWarning("No GitHub token - rate limits apply");
    }

    /// <summary>
    /// Enriches the timeline with GitHub pull requests.
    /// </summary>
    /// <param name="timeline">Timeline to enrich</param>
    /// <param name="repositoryId">Repository identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of PRs processed, matched, and unmatched in <see cref="EnrichmentResult"/></returns>
    public override async Task<Result<EnrichmentResult>> EnrichAsync(
        Timeline timeline,
        RepositoryId repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Fetching PRs from {Repo}", repositoryId.FullName);

            var allPRs = await FetchAllPullRequestsAsync(repositoryId.Owner, repositoryId.Name, cancellationToken);

            if (!allPRs.Any())
            {
                Logger.LogWarning("No PRs found");
                return Result<EnrichmentResult>.Success(new EnrichmentResult(0, 0, 0));
            }

            int matched = 0;

            foreach (var pr in allPRs)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Result<EnrichmentResult>.Failure("Cancelled");

                var targetEvent = FindMatchingEvent(timeline, pr);
                if (targetEvent == null) continue;
                
                var prNumber = PullRequestNumber.Create(pr.Number).Value;
                var prType = MapPrState(pr.Merged, pr.State.StringValue);
                var metadata = $"PR#{pr.Number} by {pr.User.Login} -> {pr.Base.Ref}";

                EnrichTraceEventWithPr(targetEvent, prNumber, prType, metadata);
                matched++;
            }

            Logger.LogInformation("Enrichment complete: {Matched}/{Total} matched", matched, allPRs.Count);
            return Result<EnrichmentResult>.Success(new EnrichmentResult(allPRs.Count, matched, allPRs.Count - matched));
        }
        catch (Exception ex) when (ex is RateLimitExceededException or NotFoundException)
        {
            var message = ex switch
            {
                RateLimitExceededException => "GitHub rate limit exceeded",
                NotFoundException => "Repository not found",
                _ => "GitHub enrichment failed"
            };
            
            Logger.LogError(ex, message);
            return Result<EnrichmentResult>.Failure(message, ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "GitHub enrichment failed");
            return Result<EnrichmentResult>.Failure("Enrichment failed", ex);
        }
    }

    /// <summary>
    /// Fetches all pull requests for the repository, handling pagination.
    /// </summary>
    private async Task<List<PullRequest>> FetchAllPullRequestsAsync(string owner, string repoName, CancellationToken cancellationToken)
    {
        var allPRs = new List<PullRequest>();
        int page = 1;

        while (!cancellationToken.IsCancellationRequested)
        {
            Logger.LogDebug("Fetching page {Page}", page);

            var prs = await _client.PullRequest.GetAllForRepository(
                owner,
                repoName,
                new PullRequestRequest { State = ItemStateFilter.All },
                new ApiOptions { PageCount = 1, PageSize = 100, StartPage = page }
            );

            if (!prs.Any())
                break;

            allPRs.AddRange(prs);
            page++;
        }

        return allPRs;
    }

    /// <summary>
    /// Attempts to find the timeline event that matches the given PR via merge SHA, head SHA, or branch name.
    /// </summary>
    private TraceEvent? FindMatchingEvent(Timeline timeline, PullRequest pr)
    {
        TraceEvent? targetEvent = null;

        // 1. Merge commit SHA
        if (!string.IsNullOrEmpty(pr.MergeCommitSha))
        {
            var shaResult = CommitSha.Create(pr.MergeCommitSha);
            if (shaResult.IsSuccess)
            {
                targetEvent = timeline.FindFirst(e => e.CommitSha != null && e.CommitSha.Matches(shaResult.Value));
                if (targetEvent != null)
                    Logger.LogDebug("PR #{Number} matched via merge SHA", pr.Number);
            }
        }

        // 2. Head SHA
        if (targetEvent == null && pr.Head?.Sha != null)
        {
            var shaResult = CommitSha.Create(pr.Head.Sha);
            if (shaResult.IsSuccess)
            {
                targetEvent = timeline.FindFirst(e => e.CommitSha != null && e.CommitSha.Matches(shaResult.Value));
                if (targetEvent != null)
                    Logger.LogDebug("PR #{Number} matched via head SHA", pr.Number);
            }
        }

        // 3. Branch name
        if (targetEvent == null && !string.IsNullOrEmpty(pr.Head?.Ref))
        {
            var branchResult = BranchName.Create(pr.Head.Ref);
            if (branchResult.IsSuccess)
            {
                targetEvent = timeline.FindFirst(e => e.BranchName != null && e.BranchName == branchResult.Value);
                if (targetEvent != null)
                    Logger.LogDebug("PR #{Number} matched via branch", pr.Number);
            }
        }

        return targetEvent;
    }
}