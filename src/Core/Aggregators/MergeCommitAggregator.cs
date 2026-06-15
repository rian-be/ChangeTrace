using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Interfaces;

namespace ChangeTrace.Core.Aggregators;

/// <summary>
/// Emits merge semantic events by combining merge metadata with commit bundles.
/// </summary>
internal sealed class MergeCommitAggregator(
    SemanticEventWriter<MergeEvent> writer,
    MergeMetadataAggregator metadata)
    : IEventAggregator<CommitBundleEvent>
{
    public void Process(CommitBundleEvent bundle)
    {
        if (!metadata.TryTake(bundle.CommitSha, out var merge))
            return;

        writer.Write(new MergeEvent(
            merge.Timestamp,
            merge.Actor,
            merge.SourceBranch,
            merge.TargetBranch,
            bundle.Files));
    }

    public void Flush()
    {
        metadata.Clear();
    }
}
