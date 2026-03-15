using ChangeTrace.Core.Events.Semantic;

namespace ChangeTrace.Core.Interfaces;

/// <summary>
/// Defines contract for aggregating and transforming semantic events from type <typeparamref name="TIn"/> to <typeparamref name="TOut"/>.
/// </summary>
/// <typeparam name="TIn">The input event type to be processed.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Implementations receive events of type <typeparamref name="TIn"/> and write transformed events into <see cref="SemanticEventWriter{TOut}"/>.</item>
/// <item>The <see cref="Flush"/> method allows implementations to write any pending events to writer.</item>
/// </list>
/// </remarks>
internal interface IEventAggregator<in TIn>
{
    /// <summary>
    /// Processes a single input event and writes zero or more output events to provided writers.
    /// </summary>
    /// <param name="evt">The input event to process.</param>
    void Process(TIn evt);

    /// <summary>
    /// Flushes any pending events into the provided writer.
    /// </summary>
    void Flush();
}