using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Processors;
using Xunit;

namespace ChangeTrace.Tests.Rendering.Processors;

/// <summary>Tests semantic aggregation stage chaining across trace and commit-level aggregators.</summary>
public sealed class TraceEventAggregationStageTests
{
    /// <summary>Flush forwards newly completed commit bundles into commit-level aggregators.</summary>
    [Fact]
    public void Flush_ProcessesFileCouplingFromCompletedCommitBundles()
    {
        using var stage = new TraceEventAggregationStage(
            RenderEventKinds.Commit |
            RenderEventKinds.FileCoupling);

        var timestamp = Timestamp.Create(1_735_689_600).Value;
        var actor = ActorName.Create("rian").Value;
        var sha = CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value;

        stage.Process(TraceEventFactory.FileChange(
            timestamp,
            actor,
            FilePath.Create("src/Core/A.cs").Value,
            FileChangeKind.Modified,
            sha));
        stage.Process(TraceEventFactory.FileChange(
            timestamp,
            actor,
            FilePath.Create("src/Core/B.cs").Value,
            FileChangeKind.Modified,
            sha));
        stage.Process(TraceEventFactory.Commit(
            timestamp,
            actor,
            sha));

        stage.Flush();

        Assert.Equal(1, stage.GetWriter<CommitBundleEvent>().Count);
        Assert.Equal(1, stage.GetWriter<FileCouplingEvent>().Count);
    }

    /// <summary>Flush combines merge metadata with commit bundles to emit merge semantic events.</summary>
    [Fact]
    public void Flush_ProcessesMergeFromCommitBundles()
    {
        using var stage = new TraceEventAggregationStage(
            RenderEventKinds.Commit |
            RenderEventKinds.Merge);

        var timestamp = Timestamp.Create(1_735_689_600).Value;
        var actor = ActorName.Create("rian").Value;
        var sha = CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value;
        var branch = BranchName.Create("main").Value;

        stage.Process(TraceEventFactory.FileChange(
            timestamp,
            actor,
            FilePath.Create("src/Core/A.cs").Value,
            FileChangeKind.Modified,
            sha));
        stage.Process(TraceEventFactory.FileChange(
            timestamp,
            actor,
            FilePath.Create("src/Core/B.cs").Value,
            FileChangeKind.Modified,
            sha));
        stage.Process(TraceEventFactory.Merge(
            timestamp,
            actor,
            sha,
            branch));

        stage.Flush();

        Assert.Equal(1, stage.GetWriter<CommitBundleEvent>().Count);
        Assert.Equal(1, stage.GetWriter<MergeEvent>().Count);
    }
}
