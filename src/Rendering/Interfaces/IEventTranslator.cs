using ChangeTrace.Core.Events;
using ChangeTrace.Rendering.Commands;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Defines translator that converts <see cref="TraceEvent"/> objects into one or more <see cref="RenderCommand"/>s
/// for rendering in visualization system.
/// </summary>
/// <remarks>
/// Implementations should decide whether they can handle a given event via <see cref="CanHandle(TraceEvent)"/>,
/// and produce corresponding commands via <see cref="Translate(TraceEvent)"/>.
/// <para>
/// Translators are typically used in <see cref="ITranslationPipeline"/> to process events in order of priority.
/// </para>
/// </remarks>
internal interface IEventTranslator
{
    /// <summary>
    /// Determines whether this translator can handle specified event.
    /// </summary>
    /// <param name="evt">The <see cref="TraceEvent"/> to check.</param>
    /// <returns><c>true</c> if this translator can handle event; otherwise, <c>false</c>.</returns>
    bool CanHandle(TraceEvent evt);

    /// <summary>
    /// Translates specified <see cref="TraceEvent"/> into one or more <see cref="RenderCommand"/> objects.
    /// </summary>
    /// <param name="evt">The event to translate.</param>
    /// <returns>An enumerable of <see cref="RenderCommand"/> objects generated from event.</returns>
    IEnumerable<RenderCommand> Translate(TraceEvent evt);
}