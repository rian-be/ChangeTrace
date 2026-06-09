using System.Net;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Enrichers;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using ChangeTrace.GIt.Services.Checkpoints.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Octokit;
using Xunit;

namespace ChangeTrace.Tests.GIt.Enrichers;

public sealed class GitHubEnricherTests
{
    [Fact]
    public async Task EnrichAsync_SkipsPullRequestEnrichmentWhenGitHubReturnsNotFound()
    {
        var checkpointStore = new TestExportCheckpointStore();
        var enricher = new NotFoundGitHubEnricher(checkpointStore);
        var repositoryId = RepositoryId.Create("torvalds", "linux").Value;
        var timeline = new Timeline(repositoryId);
        timeline.AddEvent(TraceEventFactory.Commit(
            Timestamp.Create(1_735_689_600).Value,
            ActorName.Create("linus").Value,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            "metadata"));

        var result = await enricher.Enrich(timeline, repositoryId, new ExportOptions
        {
            CheckpointKey = "/tmp/linux.gittrace",
            CheckpointFingerprint = "fingerprint"
        });

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(0, result.Value.TotalPullRequests);
        Assert.Equal(0, result.Value.MatchedCount);
        Assert.Single(timeline.Events);
        Assert.Null(timeline.Events[0].PullRequest);
        Assert.Collection(
            checkpointStore.SavedCheckpoints,
            checkpoint => Assert.Equal(ExportCheckpointStage.Enriched, checkpoint.Stage));
    }

    private sealed class NotFoundGitHubEnricher(IExportCheckpointStore checkpointStore)
        : GitHubEnricher(NullLogger<GitHubEnricher>.Instance, checkpointStore)
    {
        protected override Task<IReadOnlyList<PullRequest>> FetchPullRequestsPage(
            GitHubClient client,
            RepositoryId repositoryId,
            int page,
            CancellationToken cancellationToken)
            => throw new NotFoundException("repos/torvalds/linux/pulls was not found.", HttpStatusCode.NotFound);
    }

    private sealed class TestExportCheckpointStore : IExportCheckpointStore
    {
        public List<ExportCheckpointState> SavedCheckpoints { get; } = [];

        public Task<ExportCheckpointState?> TryLoad(
            string checkpointKey,
            string expectedFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult<ExportCheckpointState?>(null);

        public Task Save(
            string checkpointKey,
            ExportCheckpointState state,
            CancellationToken cancellationToken = default)
        {
            SavedCheckpoints.Add(state);
            return Task.CompletedTask;
        }

        public Task AppendPullRequestPatch(
            string checkpointKey,
            ExportCheckpointState state,
            int targetIndex,
            TraceEvent updatedEvent,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Clear(string checkpointKey, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
