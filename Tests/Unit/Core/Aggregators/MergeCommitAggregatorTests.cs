using ChangeTrace.Core.Aggregators;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Models;
using Xunit;

namespace ChangeTrace.Tests.Core.Aggregators;

/// <summary>Tests merge aggregation built from merge metadata and commit bundles.</summary>
public sealed class MergeCommitAggregatorTests
{
    /// <summary>Completed commit bundles produce merge semantic events with bundled files.</summary>
    [Fact]
    public void Process_EmitsMergeUsingCommitBundleFiles()
    {
        using var writer = new SemanticEventWriter<MergeEvent>();
        var metadata = new MergeMetadataAggregator();
        var aggregator = new MergeCommitAggregator(writer, metadata);

        var sha = CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value;
        var timestamp = Timestamp.Create(1_735_689_600).Value;
        var actor = ActorName.Create("rian").Value;
        var branch = BranchName.Create("main").Value;

        metadata.Process(TraceEventFactory.Merge(timestamp, actor, sha, branch));
        aggregator.Process(new CommitBundleEvent(
            sha.Value,
            actor.Value,
            timestamp.UnixSeconds,
            new[] { "src/Core/A.cs", "src/Core/B.cs" }));

        var merges = writer.Snapshot().ToArray();
        Assert.Single(merges);
        Assert.Equal(timestamp.UnixSeconds, merges[0].Timestamp);
        Assert.Equal("rian", merges[0].Actor);
        Assert.Equal("main", merges[0].SourceBranch);
        Assert.Equal("main", merges[0].TargetBranch);
        Assert.Equal(["src/Core/A.cs", "src/Core/B.cs"], merges[0].FilesMerged.ToArray());
    }

    /// <summary>Non-merge commit bundles do not emit merge events.</summary>
    [Fact]
    public void Process_IgnoresCommitBundleWithoutMergeMetadata()
    {
        using var writer = new SemanticEventWriter<MergeEvent>();
        var metadata = new MergeMetadataAggregator();
        var aggregator = new MergeCommitAggregator(writer, metadata);

        aggregator.Process(new CommitBundleEvent(
            "0123456789abcdef0123456789abcdef01234567",
            "rian",
            1_735_689_600,
            new[] { "src/Core/A.cs" }));

        Assert.Equal(0, writer.Count);
    }
}
