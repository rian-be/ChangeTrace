using System.Buffers;
using System.Runtime.InteropServices;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Interfaces;

namespace ChangeTrace.Core.Aggregators;

/// <summary>
/// Aggregates <see cref="TraceEvent"/> instances belonging to the same commit
/// into single <see cref="CommitBundleEvent"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Groups file changes by commit SHA.</item>
/// <item>Collects all files modified within the commit.</item>
/// <item>Emits a <see cref="CommitBundleEvent"/> when the commit marker (<see cref="FileChangeKind.Commit"/>) is encountered.</item>
/// <item>Uses pooled buffers (<see cref="ArrayPool{T}"/>) to reduce allocations during file collection.</item>
/// </list>
/// </remarks>
internal sealed class CommitBundlingAggregator(
    SemanticEventWriter<CommitBundleEvent> writer)
    : IEventAggregator<TraceEvent>, IDisposable
{
    private readonly Dictionary<string, CommitBuilder> _builders = new(128);
    private bool _disposed;

    /// <summary>
    /// Processes a <see cref="TraceEvent"/> and aggregates it into the corresponding commit bundle.
    /// </summary>
    /// <param name="evt">The trace event to process.</param>
    public void Process(TraceEvent evt)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(CommitBundlingAggregator));
        
        if (evt.Commit is not { Sha: { } sha, Type: var type }) 
            return;

        ref var builder = ref CollectionsMarshal.GetValueRefOrAddDefault(_builders, sha, out var existed);
        if (!existed)
            builder = new CommitBuilder(evt.Core.Actor, sha, evt.Core.Timestamp.UnixSeconds);

        if (type == FileChangeKind.Commit)
        {
            builder?.IsReady = true;
            return;
        }

        if ((evt.Metadata?.FilePath?.Value ?? evt.Target) is { Length: > 0 } file)
            builder?.AddFile(file);
    }

    /// <summary>
    /// Flushes all completed commit bundles and writes them to the semantic event writer.
    /// </summary>
    public void Flush()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(CommitBundlingAggregator));

        foreach (var builder in _builders.Values)
        {
            if (!builder.IsReady || builder.FilesCount == 0) continue;
            writer.Write(builder.BuildEvent());
            builder.Clear();
        }

        _builders.Clear();
    }

    /// <summary>
    /// Disposes the aggregator and flushes remaining events.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        Flush();
        _disposed = true;
    }

    /// <summary>
    /// Internal builder used to accumulate file changes belonging to a single commit.
    /// </summary>
    private sealed class CommitBuilder(string actor, string sha, double timestamp)
    {
        private string[] _files = ArrayPool<string>.Shared.Rent(16);
        private int _count;

        private string Sha { get; } = sha;
        private string Actor { get; } = actor;
        private double Timestamp { get; } = timestamp;

        /// <summary>
        /// Indicates whether the commit marker has been observed.
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Gets the number of collected files.
        /// </summary>
        public int FilesCount => _count;

        /// <summary>
        /// Adds a file to the commit bundle.
        /// Duplicate entries are ignored.
        /// </summary>
        public void AddFile(string file)
        {
            for (var i = 0; i < _count; i++)
                if (_files[i] == file) return;

            if (_count == _files.Length)
            {
                var newArray = ArrayPool<string>.Shared.Rent(_files.Length * 2);
                _files.AsSpan(0, _count).CopyTo(newArray);
                ArrayPool<string>.Shared.Return(_files, clearArray: true);
                _files = newArray;
            }

            _files[_count++] = file;
        }

        /// <summary>
        /// Builds the final <see cref="CommitBundleEvent"/>.
        /// </summary>
        public CommitBundleEvent BuildEvent()
        {
            var files = new string[_count];
            _files.AsSpan(0, _count).CopyTo(files);
            return new CommitBundleEvent(Sha, Actor, Timestamp, files);
        }

        /// <summary>
        /// Clears the builder state and returns pooled buffers.
        /// </summary>
        public void Clear()
        {
            ArrayPool<string>.Shared.Return(_files, clearArray: true);
            _files = ArrayPool<string>.Shared.Rent(16);
            _count = 0;
            IsReady = false;
        }
    }
}