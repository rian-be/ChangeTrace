using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Enrichers;
using ChangeTrace.GIt.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ChangeTrace.Tests.GIt.Enrichers;

public sealed class GitLabEnricherTests
{
    [Fact]
    public async Task EnrichAsync_AttachesPullRequestMetadataForMatchingMergeCommit()
    {
        var enricher = new TestGitLabEnricher([
            new GitLabMergeRequestSnapshot(
                42,
                "0123456789abcdef0123456789abcdef01234567",
                "0123456789abcdef0123456789abcdef01234567",
                "feature/trace",
                "main",
                "merged",
                new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                "rian")
        ]);

        var repositoryId = RepositoryId.Create("rian", "changetrace").Value;
        var timeline = new Timeline(repositoryId);
        var sha = CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value;
        var actor = ActorName.Create("rian").Value;

        timeline.AddEvent(TraceEventFactory.Commit(
            Timestamp.Create(1_735_689_600).Value,
            actor,
            sha,
            "metadata"));

        var result = await enricher.EnrichAsync(timeline, repositoryId);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(1, result.Value.MatchedCount);

        var enriched = timeline.Events[0];
        Assert.Equal(42, enriched.PullRequest?.Number.Value);
        Assert.Equal("MR!42 by rian -> main", enriched.Metadata?.Metadata);
    }

    private sealed class TestGitLabEnricher(IReadOnlyList<GitLabMergeRequestSnapshot> mergeRequests)
        : GitLabEnricher(Options.Create(new ChangeTrace.GIt.Options.ExportOptions()), NullLogger<GitLabEnricher>.Instance)
    {
        protected override IEnumerable<GitLabMergeRequestSnapshot> GetMergeRequests(
            RepositoryId repositoryId,
            CancellationToken cancellationToken = default)
            => mergeRequests;
    }
}
