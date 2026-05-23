using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Processors;

namespace ChangeTrace.Rendering.States.AggregationStage;

/// <summary>
/// Module responsible for rendering branch related events into scene commands.
/// </summary>
/// <remarks>
/// <para>
/// This module processes <see cref="BranchEvent"/> instances from a
/// <see cref="SemanticEventWriter{BranchEvent}"/> and dispatches the resulting
/// <see cref="RenderCommand"/> objects to the scene using
/// <see cref="SceneCommandDispatcher"/>.
/// </para>
/// <para>
/// It ensures that each branch event is processed only once per branch and event type.
/// </para>
/// <para>
/// The module implements <see cref="IRenderEventModule"/>, so it can be integrated
/// into the rendering pipeline alongside other modules.
/// </para>
/// </remarks>
internal sealed class BranchRenderModule(SemanticEventWriter<BranchEvent> writer) : IRenderEventModule
{
    private readonly HashSet<(string branch, BranchEventType type)> _processed = [];

    /// <summary>
    /// Dispatches all unprocessed branch events to scene via provided translation pipeline
    /// and command dispatcher.
    /// </summary>
    /// <param name="translation">
    /// Pipeline used to translate <see cref="BranchEvent"/> instances into <see cref="RenderCommand"/>.
    /// </param>
    /// <param name="dispatcher">
    /// Dispatcher responsible for executing the translated render commands in the scene.
    /// </param>
    public void Dispatch(
        ITranslationPipeline translation,
        SceneCommandDispatcher dispatcher)
    {
        var snapshot = writer.Snapshot();

        foreach (var evt in snapshot.Span)
        {
            if (!_processed.Add((evt.Branch, evt.Type)))
                continue;

            foreach (var cmd in translation.Translate(evt))
                dispatcher.Dispatch(cmd, evt.Timestamp);
        }
    }

    /// <summary>
    /// Clears module state, including processed branches and writer buffer.
    /// </summary>
    public void Clear()
    {
        _processed.Clear();
        writer.Clear();
    }
}