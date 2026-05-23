using ChangeTrace.Player;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Assembles immutable render state snapshots.
/// </summary>
internal interface IRenderStateAssembler
{
    /// <summary>
    /// Records actor activity event.
    /// </summary>
    void RecordActorEvent(
        string actor,
        string commitSha);

    /// <summary>
    /// Builds render a state snapshot for the current frame.
    /// </summary>
    RenderState Assemble(
        double virtualTime,
        double wallDelta,
        ISceneGraph scene,
        IAnimationSystem anim,
        Camera.Camera camera,
        ICameraController cameraCtrl,
        PlayerDiagnostics diagnostics,
        SceneNode? hoveredNode,
        HoveredPodHud? hoveredPod,
        LayoutMode mode);

    /// <summary>
    /// Clears accumulated internal state.
    /// </summary>
    void Reset();
}