using ChangeTrace.Core.Interfaces;

namespace ChangeTrace.Core.Aggregators;

/// <summary>
/// Engine responsible for executing set of event aggregators over stream of input events.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Processes events sequentially through all registered <see cref="IEventAggregator{TIn}"/> instances.</item>
/// <item>Supports both streaming mode (single event) and batch mode (multiple events).</item>
/// <item>Automatically flushes aggregators when batch size threshold is reached.</item>
/// <item>Ensures final flush during disposal.</item>
/// </list>
/// </remarks>
internal sealed class EventAggregationEngine<TIn> : IDisposable
{
    private readonly IEventAggregator<TIn>[] _aggregators;
    private readonly IDisposable[] _disposables;
    private readonly int _batchSize;

    /// <summary>
    /// Initializes a new instance of the aggregation engine.
    /// </summary>
    /// <param name="aggregators">Aggregators that will process incoming events.</param>
    /// <param name="batchSize">Number of events processed before automatic flush.</param>
    public EventAggregationEngine(
        IEnumerable<IEventAggregator<TIn>> aggregators,
        int batchSize = 1024)
    {
        _aggregators = aggregators.ToArray();
        _disposables = _aggregators.OfType<IDisposable>().ToArray();
        _batchSize = batchSize;
    }

    /// <summary>
    /// Processes span of events through all registered aggregators.
    /// </summary>
    /// <remarks>
    /// Streaming mode (single event) disables automatic flushing to reduce overhead.
    /// </remarks>
    /// <param name="events">Events to process.</param>
    public void Process(ReadOnlySpan<TIn> events)
    {
        if (events.Length == 0)
            return;

        var isStreamingMode = events.Length == 1;
        var batchCounter = 0;
        
        foreach (ref readonly var evt in events)
        {
            foreach (var aggregator in _aggregators)
                aggregator.Process(evt);

            batchCounter++;

            if (isStreamingMode || batchCounter < _batchSize) continue;
            Flush();
            batchCounter = 0;
        }

        if (!isStreamingMode && batchCounter > 0)
        {
            Flush();
        }
    }

    /// <summary>
    /// Flushes all registered aggregators.
    /// </summary>
    public void Flush()
    {
        foreach (var agg in _aggregators)
            agg.Flush();
    }

    /// <summary>
    /// Flushes and disposes all aggregators that implement <see cref="IDisposable"/>.
    /// </summary>
    public void Dispose()
    {
        Flush();
        
        foreach (var d in _disposables)
            d.Dispose();
    }
}