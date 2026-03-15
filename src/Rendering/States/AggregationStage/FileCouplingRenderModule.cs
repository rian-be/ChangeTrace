using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Processors;

namespace ChangeTrace.Rendering.States.AggregationStage;

/// <summary>
/// Module responsible for rendering file coupling events into scene commands.
/// </summary>
/// <remarks>
/// <para>
/// This module processes <see cref="FileCouplingEvent"/> instances from
/// <see cref="SemanticEventWriter{FileCouplingEvent}"/> and dispatches the resulting
/// <see cref="RenderCommand"/> objects to scene via a <see cref="SceneCommandDispatcher"/>.
/// </para>
/// <para>
/// It ensures that each unique file pair is processed only once, avoiding duplicate
/// commands for the same file coupling.
/// </para>
/// <para>
/// Implements <see cref="IRenderEventModule"/> so it can be integrated into
/// the rendering pipeline alongside other modules.
/// </para>
/// </remarks>
internal sealed class FileCouplingRenderModule(SemanticEventWriter<FileCouplingEvent> writer) : IRenderEventModule
{
    private readonly HashSet<(string,string)> _processed = [];

    /// <summary>
    /// Dispatches all unprocessed file coupling events to the scene using the provided
    /// translation pipeline and command dispatcher.
    /// </summary>
    /// <param name="translation">
    /// The pipeline used to translate <see cref="FileCouplingEvent"/> instances into <see cref="RenderCommand"/>.
    /// </param>
    /// <param name="dispatcher">
    /// The dispatcher responsible for executing the translated commands in the scene.
    /// </param>
    public void Dispatch(
        ITranslationPipeline translation,
        SceneCommandDispatcher dispatcher)
    {
        var snapshot = writer.Snapshot();

        foreach (var evt in snapshot.Span)
        {
            if (!_processed.Add((evt.FileA, evt.FileB)))
                continue;

            foreach (var cmd in translation.Translate(evt))
                dispatcher.Dispatch(cmd, evt.Timestamp);
        }
    }

    /// <summary>
    /// Clears module state, including processed file pairs
    /// and writer buffer.
    /// </summary>
    public void Clear()
    {
        _processed.Clear();
        writer.Clear();
    }
}