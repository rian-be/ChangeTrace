using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Enrichers;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using ChangeTrace.GIt.Services;
using Xunit;

namespace ChangeTrace.Tests.GIt.Services;

/// <summary>Tests provider enricher resolution.</summary>
public sealed class TimelineEnricherResolverTests
{
    /// <summary>Resolver returns the enricher registered for GitHub.</summary>
    [Fact]
    public void TryResolve_ReturnsGithubEnricher()
    {
        var resolver = new TimelineEnricherResolver([new TestProviderEnricher("github")]);

        var resolved = resolver.TryResolve("github", out var enricher);

        Assert.True(resolved);
        Assert.NotNull(enricher);
        Assert.Equal("github", ((IProviderTimelineEnricher)enricher!).Provider);
    }

    /// <summary>Resolver rejects unknown providers.</summary>
    [Fact]
    public void TryResolve_RejectsUnknownProvider()
    {
        var resolver = new TimelineEnricherResolver([new TestProviderEnricher("github")]);

        var resolved = resolver.TryResolve("gitlab", out var enricher);

        Assert.False(resolved);
        Assert.Null(enricher);
    }

    private sealed class TestProviderEnricher(string provider) : IProviderTimelineEnricher
    {
        public string Provider => provider;

        public Task<Result<EnrichmentResult>> Enrich(
            Timeline timeline,
            RepositoryId repositoryId,
            ExportOptions options,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result<EnrichmentResult>.Success(new EnrichmentResult(0, 0, 0)));
    }
}
