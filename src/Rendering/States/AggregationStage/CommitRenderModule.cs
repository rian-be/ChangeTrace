using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Processors;

namespace ChangeTrace.Rendering.States.AggregationStage;

/// <summary>
/// Module responsible for rendering commit related events into scene commands.
/// </summary>
/// <remarks>
/// <para>
/// This module processes <see cref="CommitBundleEvent"/> instances from
/// <see cref="SemanticEventWriter{CommitBundleEvent}"/> and dispatches resulting
/// <see cref="RenderCommand"/> objects to scene via  <see cref="SceneCommandDispatcher"/>.
/// </para>
/// <para>
/// It ensures that each commit SHA is processed only once, avoiding duplicate commands for the same commit.
/// </para>
/// <para>
/// Implements <see cref="IRenderEventModule"/> to integrate with the rendering pipeline.
/// </para>
/// </remarks>
internal sealed class CommitRenderModule(SemanticEventWriter<CommitBundleEvent> writer) : IRenderEventModule
{
    private readonly HashSet<string> _processed = [];

    /// <summary>
    /// Dispatches all unprocessed commit events to scene using provided translation
    /// pipeline and command dispatcher.
    /// </summary>
    /// <param name="translation">
    /// The pipeline used to translate <see cref="CommitBundleEvent"/> instances into <see cref="RenderCommand"/>.
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
            if (!_processed.Add(evt.CommitSha))
                continue;

            foreach (var cmd in translation.Translate(evt))
                dispatcher.Dispatch(cmd, evt.Timestamp);
        }
    }

    /// <summary>
    /// Clears module state, including processed commit SHAs and the writer buffer.
    /// </summary>
    public void Clear()
    {
        _processed.Clear();
        writer.Clear();
    }
}