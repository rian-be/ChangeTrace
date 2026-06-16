using ChangeTrace.Core.Events;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Processors;

namespace ChangeTrace.Rendering.Pipeline;

/// <summary>
/// Buffers playback events and flushes aggregated render events.
/// </summary>
internal sealed class RenderEventBuffer(RenderEventKinds renderEvents) : IDisposable
{
    private readonly Lock _sync = new();

    private TraceEventAggregationStage _aggregation = new(
        renderEvents);

    private RenderEventKinds _renderEvents = renderEvents;
    private bool _hasBufferedEvents;

    /// <summary>
    /// Updates enabled render event kinds and resets aggregation state.
    /// </summary>
    public void SetRenderEvents(
        RenderEventKinds renderEvents)
    {
        lock (_sync)
        {
            if (_renderEvents == renderEvents)
                return;

            _renderEvents = renderEvents;

            _aggregation.Dispose();

            _aggregation = new TraceEventAggregationStage(
                renderEvents);
        }
    }

    /// <summary>
    /// Adds a trace event to the aggregation buffer.
    /// </summary>
    public void Add(
        TraceEvent evt)
    {
        lock (_sync)
        {
            _aggregation.Process(
                evt);
            _hasBufferedEvents = true;
        }
    }

    /// <summary>
    /// Flushes buffered events into the rendering pipeline.
    /// </summary>
    public bool FlushTo(
        RenderingPipeline pipeline)
    {
        lock (_sync)
        {
            if (!_hasBufferedEvents)
                return false;

            _aggregation.Flush();

            foreach (var t in RenderEventDispatchTable.Table)
            {
                ref readonly var entry = ref t;

                if ((_renderEvents & entry.Kind) == 0)
                    continue;

                entry.Fn(
                    pipeline,
                    _aggregation);
            }

            _aggregation.Clear();
            _hasBufferedEvents = false;
            return true;
        }
    }

    /// <summary>
    /// Releases aggregation resources.
    /// </summary>
    public void Dispose()
    {
        lock (_sync)
        {
            _aggregation.Dispose();
        }
    }
}
