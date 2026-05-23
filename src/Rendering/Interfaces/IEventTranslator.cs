using ChangeTrace.Rendering.Commands;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Translates trace events into render commands.
/// </summary>
internal interface IEventTranslator
{
    /// <summary>
    /// Event type handled by this translator.
    /// </summary>
    Type EventType { get; }

    /// <summary>
    /// Determines whether the translator can handle the event.
    /// </summary>
    bool CanHandle(object evt);

    /// <summary>
    /// Translates event into render commands.
    /// </summary>
    IEnumerable<RenderCommand> Translate(object evt);
}