using ChangeTrace.Core.Events.Semantic;

namespace ChangeTrace.Core.Interfaces;

/// <summary>
/// Defines contract for aggregating and transforming semantic events from type <typeparamref name="TIn"/> to <typeparamref name="TOut"/>.
/// </summary>
/// <typeparam name="TIn">The input event type to be processed.</typeparam>
/// <typeparam name="TOut">The output event type produced by the aggregator.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Implementations receive events of type <typeparamref name="TIn"/> and write transformed events into <see cref="SemanticEventWriter{TOut}"/>.</item>
/// <item>The <see cref="Flush"/> method allows implementations to write any pending events to writer.</item>
/// </list>
/// </remarks>
internal interface IEventAggregator<TIn, TOut>
{
    /// <summary>
    /// Processes a single input event and writes zero or more output events to provided writers.
    /// </summary>
    /// <param name="evt">The input event to process.</param>
    /// <param name="writer">The semantic event writer where output events are written.</param>
    void Process(in TIn evt, SemanticEventWriter<TOut> writer);

    /// <summary>
    /// Flushes any pending events into the provided writer.
    /// </summary>
    /// <param name="writer">The semantic event writer where pending output events are written.</param>
    void Flush(SemanticEventWriter<TOut> writer);
}