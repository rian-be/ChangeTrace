using System.Runtime.CompilerServices;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Services;

namespace ChangeTrace.Core.Aggregators;

/// <summary>
/// Aggregator that processes <see cref="TraceEvent"/> instances and emits semantic <see cref="BranchEvent"/> objects.
/// </summary>
/// <remarks>
/// <para>
/// This aggregator tracks branch creation and deletion based on incoming trace events. It uses a <see cref="BranchTracker"/>
/// to maintain branch state across events, and ensures that branch events are only emitted once per branch lifecycle.
/// </para>
/// <para>
/// On <see cref="Process"/> it emits a <see cref="BranchEventType.BranchCreated"/> event for newly seen branches.
/// On <see cref="Flush"/>, it emits <see cref="BranchEventType.BranchDeleted"/> events for branches no longer active.
/// </para>
/// </remarks>
internal sealed class BranchAggregator(SemanticEventWriter<BranchEvent> writer) : IEventAggregator<TraceEvent>
{
    private readonly BranchTracker _tracker = new();
    private static readonly HashSet<string> EmptyBranchSet = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Processes single <see cref="TraceEvent"/> and emits semantic branch events if needed.
    /// </summary>
    /// <param name="evt">The trace event to process.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Process(TraceEvent evt)
    {
        var branchName = evt.Branch?.Name.Value;
        if (branchName == null)
            return;
        
        var timestamp = evt.Core.Timestamp;
        
        bool isNew = _tracker.TryUpdate(branchName, evt.Commit?.Sha ?? null, timestamp);
        if (!isNew) return;
        
        var branchEvt = new BranchEvent(
            timestamp,
            evt.Core.Actor,
            branchName,
            BranchEventType.BranchCreated,
            sha: evt.Commit?.Sha
        );

        writer.Write(branchEvt);
    }

    /// <summary>
    /// Flushes the aggregator, emitting branch deleted events for branches no longer active.
    /// </summary>
    public void Flush()
    {
        using var pooled = _tracker.GetDeletedPooled(EmptyBranchSet);
        if (!pooled.Span.IsEmpty)
        {
            //ProcessDeletions(pooled.Span); //todo: make impl deletion detection
        }
    }
    
    /// <summary>
    /// Processes branch deletions using the timestamp of the last commit on each branch.
    /// </summary>
    /// <param name="deletions">
    /// A span of tuples containing branch name, last commit SHA, and last commit timestamp.
    /// </param>
    private void ProcessDeletions(ReadOnlySpan<(string Name, CommitSha LastSha, Timestamp LastTimestamp)> deletions)
    {
        foreach (var (name, sha, timestamp) in deletions)
        {
            var branchEvt = new BranchEvent(
                timestamp,
                "unknown",  
                name,
                BranchEventType.BranchDeleted,
                sha
            );

            writer.Write(branchEvt);
        }
    }
}