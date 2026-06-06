using ChangeTrace.Core.Aggregators;
using ChangeTrace.Core.Events.Semantic;
using Xunit;

namespace ChangeTrace.Tests.Core.Aggregators;

/// <summary>Tests file coupling event generation from commit bundles.</summary>
public sealed class FileCouplingAggregatorTests
{
    /// <summary>Process emits every unique unordered file pair in a bundle.</summary>
    [Fact]
    public void Process_EmitsEveryUniqueFilePair()
    {
        using var writer = new SemanticEventWriter<FileCouplingEvent>();
        var aggregator = new FileCouplingAggregator(writer);

        aggregator.Process(new CommitBundleEvent(
            "commit-1",
            "rian",
            123,
            new[] { "a.cs", "b.cs", "c.cs" }));

        var events = writer.Snapshot().ToArray();
        Assert.Equal(3, events.Length);
        Assert.Equal(("a.cs", "b.cs"), (events[0].FileA, events[0].FileB));
        Assert.Equal(("a.cs", "c.cs"), (events[1].FileA, events[1].FileB));
        Assert.Equal(("b.cs", "c.cs"), (events[2].FileA, events[2].FileB));
    }

    /// <summary>Process skips bundles that do not contain enough files to form a pair.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Process_DoesNotEmitPairsForSmallBundles(int fileCount)
    {
        using var writer = new SemanticEventWriter<FileCouplingEvent>();
        var aggregator = new FileCouplingAggregator(writer);
        var files = Enumerable.Range(0, fileCount)
            .Select(index => $"file-{index}.cs")
            .ToArray();

        aggregator.Process(new CommitBundleEvent("commit-1", "rian", 123, files));

        Assert.Equal(0, writer.Count);
    }
}
