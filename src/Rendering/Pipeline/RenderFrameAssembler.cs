using ChangeTrace.Player;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Pipeline;

/// <summary>
/// Assembles the render state and submits it to the render output.
/// </summary>
internal sealed class RenderFrameAssembler(
    IRenderStateAssembler assembler,
    IRenderOutput output)
{
    /// <summary>
    /// Builds and submits a frame for the current scene state.
    /// </summary>
    public void SubmitFrame(
        double positionSeconds,
        float deltaTime,
        ISceneGraph scene,
        IAnimationSystem animation,
        Camera.Camera camera,
        ICameraController cameraController,
        PlayerDiagnostics diagnostics,
        SceneNode? hoveredNode,
        HoveredPodHud? hoveredPod,
        LayoutMode layoutMode)
    {
        var state =
            assembler.Assemble(
                positionSeconds,
                deltaTime,
                scene,
                animation,
                camera,
                cameraController,
                diagnostics,
                hoveredNode,
                hoveredPod,
                layoutMode);

        output.Submit(
            state);
    }
}