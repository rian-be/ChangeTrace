using ChangeTrace.Core.Aggregators;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Models;
using Xunit;

namespace ChangeTrace.Tests.Core.Aggregators;

/// <summary>Tests commit bundle aggregation from raw trace events.</summary>
public sealed class CommitBundlingAggregatorTests
{
    /// <summary>Flush emits one bundle when a commit marker closes collected file changes.</summary>
    [Fact]
    public void Flush_EmitsBundleForReadyCommitWithFiles()
    {
        using var writer = new SemanticEventWriter<CommitBundleEvent>();
        using var aggregator = new CommitBundlingAggregator(writer);
        var sha = CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value;
        var timestamp = Timestamp.Create(1_735_689_600).Value;
        var actor = ActorName.Create("rian").Value;

        aggregator.Process(TraceEventFactory.FileChange(
            timestamp,
            actor,
            FilePath.Create("src/Core/A.cs").Value,
            FileChangeKind.Modified,
            sha));
        aggregator.Process(TraceEventFactory.FileChange(
            timestamp,
            actor,
            FilePath.Create("src/Core/B.cs").Value,
            FileChangeKind.Added,
            sha));
        aggregator.Process(TraceEventFactory.Commit(timestamp, actor, sha));

        aggregator.Flush();

        var bundles = writer.Snapshot().ToArray();
        Assert.Single(bundles);
        Assert.Equal(sha.Value, bundles[0].CommitSha);
        Assert.Equal("rian", bundles[0].Actor);
        Assert.Equal(["src/Core/A.cs", "src/Core/B.cs"], bundles[0].Files.ToArray());
    }

    /// <summary>Flush does not emit a bundle when only file changes were observed.</summary>
    [Fact]
    public void Flush_DoesNotEmitBundleBeforeCommitMarker()
    {
        using var writer = new SemanticEventWriter<CommitBundleEvent>();
        using var aggregator = new CommitBundlingAggregator(writer);

        aggregator.Process(TraceEventFactory.FileChange(
            Timestamp.Create(1_735_689_600).Value,
            ActorName.Create("rian").Value,
            FilePath.Create("src/Core/A.cs").Value,
            FileChangeKind.Modified,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value));

        aggregator.Flush();

        Assert.Equal(0, writer.Count);
    }
}
