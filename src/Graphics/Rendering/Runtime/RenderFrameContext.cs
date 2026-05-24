using ChangeTrace.Graphics.Shaders.Registry;
using ChangeTrace.Rendering.States;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Runtime;

/// <summary>
/// Immutable per frame rendering context shared across render passes.
/// </summary>
internal readonly record struct RenderFrameContext(
    RenderState State,
    RenderResources Resources,
    Matrix3 ViewProjection,
    Matrix3 ScreenProjection,
    int ViewportWidth,
    int ViewportHeight,
    float TotalTime)
{
    /// <summary>
    /// Current camera zoom.
    /// </summary>
    public float Zoom =>
        State.Camera.Zoom;

    /// <summary>
    /// Current viewport size as vector.
    /// </summary>
    public Vector2 Viewport =>
        new(
            ViewportWidth,
            ViewportHeight);
}