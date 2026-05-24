using ChangeTrace.Rendering.Pipeline;

namespace ChangeTrace.Graphics.Input.Mouse;

/// <summary>
/// Handles mouse wheel camera zoom input.
/// </summary>
internal sealed class MouseZoomController
{
    /// <summary>
    /// Mouse wheel zoom sensitivity multiplier.
    /// </summary>
    private const float ZoomSensitivity = 0.1f;

    /// <summary>
    /// Applies mouse wheel zoom to the camera.
    /// </summary>
    public void Zoom(float wheelDelta, RenderingPipeline pipeline) =>
        pipeline.ZoomCamera(wheelDelta * ZoomSensitivity);
}