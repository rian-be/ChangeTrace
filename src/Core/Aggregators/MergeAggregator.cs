using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Specifications.Queries.Commits;
using ChangeTrace.Core.Utils;

namespace ChangeTrace.Core.Aggregators;

/// <summary>
/// Aggregates merge related <see cref="TraceEvent"/> instances and emits <see cref="MergeEvent"/> semantic events.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Detects merge commits using <see cref="CommitQueries.Merges"/> specification.</item>
/// <item>Buffers merge events until flush.</item>
/// <item>Collects all files modified within the merge commit.</item>
/// <item>Emits a single <see cref="MergeEvent"/> containing source branch, target branch, actor and affected files.</item>
/// </list>
/// </remarks>
internal sealed class MergeAggregator(SemanticEventWriter<MergeEvent> writer)
    : IEventAggregator<TraceEvent>, IDisposable
{
    private readonly PooledBuffer<TraceEvent> _buffer = new(128);
    private readonly Dictionary<CommitSha, List<string>> _commitFiles = new();

    private bool _disposed;

    /// <summary>
    /// Processes <see cref="TraceEvent"/> and collects merge related data.
    /// </summary>
    /// <param name="evt">The trace event to process.</param>
    public void Process(TraceEvent evt)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MergeAggregator));

        var sha = evt.Commit?.Sha;
        if (sha == null)
            return;

        if (CommitQueries.Merges().IsSatisfiedBy(evt))
        {
            _buffer.Add(evt);
            if (!_commitFiles.ContainsKey(sha))
                _commitFiles[sha] = [];
            return;
        }

        if (evt.Commit?.Type == FileChangeKind.Commit) return;

        if (!_commitFiles.TryGetValue(sha, out var list))
        {
            list = [];
            _commitFiles[sha] = list;
        }

        if (!CommitSha.Create(evt.Target).IsSuccess)
            list.Add(evt.Target);
    }

    /// <summary>
    /// Flushes buffered merge events and emits semantic <see cref="MergeEvent"/> instances.
    /// </summary>
    public void Flush()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MergeAggregator));

        foreach (var mergeEvt in _buffer.AsMemory().Span)
        {
            var sha = mergeEvt.Commit!.Value.Sha;
            
            var files = _commitFiles.TryGetValue(sha, out var fileList)
                ? fileList.ToArray()
                : Array.Empty<string>();

            var evt = new MergeEvent(
                mergeEvt.Core.Timestamp.UnixSeconds,
                mergeEvt.Core.Actor!.Value,
                mergeEvt.Branch?.Name?.Value ?? "unknown",
                mergeEvt.Target,
                files
            );

            writer.Write(evt);
        }

        _buffer.Clear();
        _commitFiles.Clear();
    }

    /// <summary>
    /// Releases buffers used by aggregator.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _buffer.Dispose();
        _disposed = true;
    }
}