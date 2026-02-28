using ChangeTrace.Player;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Defines contract for assembling immutable <see cref="RenderState"/> snapshots.
/// </summary>
/// <remarks>
/// Implementations collect runtime data from scene, animation system,
/// camera, and player diagnostics to produce a complete render frame state.
/// </remarks>
internal interface IRenderStateAssembler
{
    /// <summary>
    /// Records an event occurrence for specified actor.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    void RecordActorEvent(string actor);

    /// <summary>
    /// Assembles new immutable render state snapshot.
    /// </summary>
    /// <param name="virtualTime">Current virtual timeline time (seconds).</param>
    /// <param name="wallDelta">Elapsed real (wall clock) time since last frame.</param>
    /// <param name="scene">Current scene graph.</param>
    /// <param name="anim">Active animation system.</param>
    /// <param name="camera">Camera state.</param>
    /// <param name="diagnostics">Current player diagnostics snapshot.</param>
    /// <returns>Newly constructed <see cref="RenderState"/> instance.</returns>
    RenderState Assemble(
        double virtualTime,
        double wallDelta,
        ISceneGraph scene,
        IAnimationSystem anim,
        Camera.Camera camera,
        PlayerDiagnostics diagnostics);

    /// <summary>
    /// Clears internal actor counters and accumulated state.
    /// </summary>
    void Reset();
}