using ChangeTrace.Rendering.Processors;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Processes events and dispatches render commands.
/// </summary>
internal interface IRenderEventModule
{
    /// <summary>
    /// Dispatches pending events through a translation pipeline.
    /// </summary>
    void Dispatch(
        ITranslationPipeline translation,
        SceneCommandDispatcher dispatcher);

    /// <summary>
    /// Clears internal buffered state.
    /// </summary>
    void Clear();
}