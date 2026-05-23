using ChangeTrace.Rendering.Commands;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Translates domain events into render commands.
/// </summary>
internal interface ITranslationPipeline
{
    /// <summary>
    /// Registers event translator with execution priority.
    /// </summary>
    void Register(
        IEventTranslator translator,
        int priority = 10);

    /// <summary>
    /// Translates event into render commands.
    /// </summary>
    IReadOnlyList<RenderCommand> Translate(object evt);
}