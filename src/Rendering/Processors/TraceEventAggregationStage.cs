using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ChangeTrace.Core.Aggregators;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Processors;

/// <summary>
/// Aggregation stage responsible for transforming <see cref="TraceEvent"/> streams
/// into higher level semantic events used by the rendering pipeline.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Runs set of <see cref="IEventAggregator{T}"/> instances for <see cref="TraceEvent"/> input.</item>
/// <item>Produces semantic events using <see cref="SemanticEventWriter{T}"/>.</item>
/// <item>Optionally executes a second aggregation stage over <see cref="CommitBundleEvent"/>.</item>
/// <item>Supports incremental processing by tracking the last processed commit bundle.</item>
/// </list>
/// </remarks>
internal sealed class TraceEventAggregationStage : IDisposable
{
    private readonly EventAggregationEngine<TraceEvent> _engine;
    private readonly EventAggregationEngine<CommitBundleEvent>? _commitEngine;

    private readonly Dictionary<Type, object> _writers = new();

    private readonly List<IEventAggregator<TraceEvent>> _traceAggregators = new();
    private readonly List<IEventAggregator<CommitBundleEvent>> _commitAggregators = new();

    private readonly SemanticEventWriter<CommitBundleEvent>? _commitWriter;

    private int _commitCursor;

    /// <summary>
    /// Initializes the aggregation stage based on enabled rendering event kinds.
    /// </summary>
    /// <param name="enabledEvents">Flags describing which event categories should be processed.</param>
    public TraceEventAggregationStage(RenderEventKinds enabledEvents)
    {
        // TRACE aggregators

        if (enabledEvents.HasFlag(RenderEventKinds.Commit) ||
            enabledEvents.HasFlag(RenderEventKinds.FileCoupling))
        {
            _commitWriter = CreateWriter<CommitBundleEvent>();
            RegisterTrace(new CommitBundlingAggregator(_commitWriter));
        }

        if (enabledEvents.HasFlag(RenderEventKinds.Branch))
            RegisterTrace(new BranchAggregator(CreateWriter<BranchEvent>()));

        if (enabledEvents.HasFlag(RenderEventKinds.Merge))
            RegisterTrace(new MergeAggregator(CreateWriter<MergeEvent>()));

        // COMMIT aggregators

        if (enabledEvents.HasFlag(RenderEventKinds.FileCoupling))
            RegisterCommit(new FileCouplingAggregator(CreateWriter<FileCouplingEvent>()));

        _engine = new EventAggregationEngine<TraceEvent>(_traceAggregators.ToArray());

        if (_commitAggregators.Count > 0)
            _commitEngine = new EventAggregationEngine<CommitBundleEvent>(_commitAggregators.ToArray());
    }

    /// <summary>
    /// Registers trace level aggregator.
    /// </summary>
    private void RegisterTrace(IEventAggregator<TraceEvent> aggregator)
        => _traceAggregators.Add(aggregator);

    /// <summary>
    /// Registers commit level aggregator.
    /// </summary>
    private void RegisterCommit(IEventAggregator<CommitBundleEvent> aggregator)
        => _commitAggregators.Add(aggregator);

    /// <summary>
    /// Creates and registers semantic event writer for the specified event type.
    /// </summary>
    private SemanticEventWriter<T> CreateWriter<T>() where T : struct
    {
        var writer = new SemanticEventWriter<T>();
        _writers[typeof(T)] = writer;
        return writer;
    }

    /// <summary>
    /// Gets semantic event writer for the specified event type.
    /// </summary>
    public SemanticEventWriter<T> GetWriter<T>() where T : struct
        => (SemanticEventWriter<T>)_writers[typeof(T)];

    /// <summary>
    /// Processes single <see cref="TraceEvent"/> through the aggregation pipeline.
    /// </summary>
    public void Process(in TraceEvent evt)
    {
        _engine.Process(
            MemoryMarshal.CreateReadOnlySpan(
                ref Unsafe.AsRef(in evt), 1));
        
        if (_commitEngine == null || _commitWriter == null)
            return;

        var snapshot = _commitWriter.Snapshot();
        var span = snapshot.Span;
        
        for (var i = _commitCursor; i < span.Length; i++)
        {
            ref readonly var commitBundle = ref span[i];

            _commitEngine.Process(
                MemoryMarshal.CreateReadOnlySpan(
                    ref Unsafe.AsRef(in commitBundle), 1));
        }

        _commitCursor = span.Length;
    }

    /// <summary>
    /// Flushes all aggregators in the pipeline.
    /// </summary>
    public void Flush()
    {
        _engine.Flush();
        _commitEngine?.Flush();
    }

    /// <summary>
    /// Clears internal state used for incremental processing.
    /// </summary>
    public void Clear()
    {
        foreach (var writer in _writers.Values)
        {
            // ((ISemanticWriter)writer).Clear(); //todo: make interface ISemanticWriter and impl
        }

        _commitCursor = 0;
    }

    /// <summary>
    /// Disposes the aggregation engines and releases resources.
    /// </summary>
    public void Dispose()
    {
        _engine.Dispose();
        _commitEngine?.Dispose();
    }
}