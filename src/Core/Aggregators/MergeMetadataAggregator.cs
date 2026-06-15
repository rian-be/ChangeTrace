using System.Runtime.InteropServices;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Interfaces;

namespace ChangeTrace.Core.Aggregators;

/// <summary>
/// Tracks merge metadata keyed by commit SHA so merge semantic events can reuse commit bundles.
/// </summary>
internal sealed class MergeMetadataAggregator : IEventAggregator<TraceEvent>
{
    private readonly Dictionary<string, MergeMetadata> _metadata = new(64);

    public void Process(TraceEvent evt)
    {
        if (evt.Commit is not { Sha.Value: var sha } ||
            evt.Branch is not { Type: BranchEventType.Merge, Name.Value: var sourceBranch })
        {
            return;
        }

        ref var metadata = ref CollectionsMarshal.GetValueRefOrAddDefault(_metadata, sha, out _);
        metadata = new MergeMetadata(
            evt.Core.Timestamp.UnixSeconds,
            evt.Core.Actor.Value,
            sourceBranch,
            evt.Target);
    }

    public void Flush()
    {
    }

    public bool TryTake(string sha, out MergeMetadata metadata)
    {
        if (_metadata.TryGetValue(sha, out metadata))
        {
            _metadata.Remove(sha);
            return true;
        }

        return false;
    }

    public void Clear() => _metadata.Clear();
}

internal readonly record struct MergeMetadata(
    double Timestamp,
    string Actor,
    string SourceBranch,
    string TargetBranch);
