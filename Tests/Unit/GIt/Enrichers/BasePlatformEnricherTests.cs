using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Enrichers;
using ChangeTrace.GIt.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ChangeTrace.Tests.GIt.Enrichers;

/// <summary>Tests common platform enricher behavior shared by Git hosting integrations.</summary>
public sealed class BasePlatformEnricherTests
{
    /// <summary>MapPrState maps merged pull requests before considering their textual state.</summary>
    [Theory]
    [InlineData(true, "open", "PullRequestMerged")]
    [InlineData(false, "closed", "PullRequestClosed")]
    [InlineData(false, "open", "PullRequestCreated")]
    [InlineData(false, "OPEN", "PullRequestCreated")]
    public void MapPrState_MapsMergedClosedAndOpenStates(
        bool merged,
        string state,
        string expected)
    {
        var actual = TestPlatformEnricher.Map(merged, state);

        Assert.Equal(expected, actual.ToString());
    }

    /// <summary>EnrichTraceEventWithPr returns an event with pull request data and merged metadata.</summary>
    [Fact]
    public void EnrichTraceEventWithPr_AttachesPullRequestAndMetadata()
    {
        var enricher = new TestPlatformEnricher();
        var traceEvent = TraceEventFactory.Commit(
            Timestamp.Create(1_735_689_600).Value,
            ActorName.Create("rian").Value,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            "Original metadata");

        var enriched = enricher.Enrich(
            traceEvent,
            42,
            PullRequestEventType.PullRequestMerged,
            "PR#42 by rian -> main");

        Assert.Equal(42, enriched.PullRequest?.Number.Value);
        Assert.Equal(PullRequestEventType.PullRequestMerged, enriched.PullRequest?.Type);
        Assert.Equal("PR#42 by rian -> main", enriched.Metadata?.Metadata);
    }

    /// <summary>Test subclass exposing protected helper methods without platform I/O.</summary>
    private sealed class TestPlatformEnricher()
        : BasePlatformEnricher(NullLogger<TestPlatformEnricher>.Instance)
    {
        /// <summary>Exposes the protected PR state mapper.</summary>
        public static PullRequestEventType Map(bool merged, string state)
            => MapPrState(merged, state);

        /// <summary>Exposes the protected event enrichment helper.</summary>
        public TraceEvent Enrich(
            TraceEvent traceEvent,
            int prNumber,
            PullRequestEventType prType,
            string metadata)
            => EnrichTraceEventWithPr(traceEvent, prNumber, prType, metadata);

        /// <summary>Completes the abstract contract without external platform calls.</summary>
        public override Task<Result<EnrichmentResult>> Enrich(
            Timeline timeline,
            RepositoryId repositoryId,
            ExportOptions options,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result<EnrichmentResult>.Success(new EnrichmentResult(0, 0, 0)));
    }
}
