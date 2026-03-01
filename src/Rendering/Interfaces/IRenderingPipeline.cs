using ChangeTrace.Player.Interfaces;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Defines rendering pipeline that drives scene updates from timeline player.
/// </summary>
/// <remarks>
/// Implementations are responsible for connecting an <see cref="ITimelinePlayer"/> to
/// rendering system, managing scene state updates, and coordinating outputs.
/// The pipeline can be started and stopped to control flow of event processing.
/// </remarks>
internal interface IRenderingPipeline : IDisposable
{
    /// <summary>
    /// Gets timeline player that drives this pipeline.
    /// </summary>
    ITimelinePlayer Player { get; }

    /// <summary>
    /// Starts pipeline, beginning event processing and scene updates.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops pipeline, halting event processing and scene updates.
    /// </summary>
    void Stop();
}