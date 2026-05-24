using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Interfaces;

/// <summary>
/// Renders scene labels and hover HUD overlays.
/// </summary>
internal interface ILabelRenderer
{
    /// <summary>
    /// Draws visible labels for the current scene snapshot.
    /// </summary>
    void Draw(
        ISceneSnapshot scene,
        CameraSnapshot camera,
        Matrix3 screenMatrix,
        int viewportWidth,
        int viewportHeight,
        string? hoveredNodeId);

    /// <summary>
    /// Draws visible labels together with hovered pod HUD content.
    /// </summary>
    void Draw(
        ISceneSnapshot scene,
        CameraSnapshot camera,
        Matrix3 screenMatrix,
        int viewportWidth,
        int viewportHeight,
        string? hoveredNodeId,
        HoveredPodHud? hoveredPod);
}