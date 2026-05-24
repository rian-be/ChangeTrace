using ChangeTrace.Rendering.Pipeline;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Input.Mouse;

/// <summary>
/// Handles zoom slider interaction and camera zoom control.
/// </summary>
internal sealed class SliderZoomController
{
    /// <summary>
    /// Slider height in screen pixels.
    /// </summary>
    private const float SliderHeight = 180f;

    /// <summary>
    /// Slider width in screen pixels.
    /// </summary>
    private const float SliderWidth = 30f;

    /// <summary>
    /// Minimum allowed zoom level.
    /// </summary>
    private const float MinZoom = 0.15f;

    /// <summary>
    /// Maximum allowed zoom level.
    /// </summary>
    private const float MaxZoom = 10.0f;

    private bool _dragging;

    /// <summary>
    /// Gets whether the zoom slider is currently being dragged.
    /// </summary>
    public bool IsDragging => _dragging;

    /// <summary>
    /// Attempts to start slider dragging.
    /// </summary>
    public bool TryBegin(Vector2 mousePosition, Vector2i viewport, RenderingPipeline pipeline)
    {
        float sliderX = viewport.X - 30f;

        float yMid = viewport.Y / 2f;
        float yTop = yMid - SliderHeight / 2f;
        float yBottom = yMid + SliderHeight / 2f;

        bool inside =
            mousePosition.X >= sliderX - 15f &&
            mousePosition.X <= sliderX + 15f &&
            mousePosition.Y >= yTop - 20f &&
            mousePosition.Y <= yBottom + 20f;

        if (!inside)
            return false;

        _dragging = true;

        UpdateZoom(mousePosition.Y, yTop, yBottom, pipeline);

        return true;
    }

    /// <summary>
    /// Ends the current slider drag operation.
    /// </summary>
    public void End() => _dragging = false;

    /// <summary>
    /// Updates zoom while dragging the slider.
    /// </summary>
    public void Update(Vector2 mousePosition, Vector2i viewport, RenderingPipeline pipeline)
    {
        if (!_dragging)
            return;

        float yMid = viewport.Y / 2f;

        float yTop = yMid - SliderHeight / 2f;
        float yBottom = yMid + SliderHeight / 2f;

        UpdateZoom(mousePosition.Y, yTop, yBottom, pipeline);
    }

    /// <summary>
    /// Converts slider position into logarithmic zoom value.
    /// </summary>
    private static void UpdateZoom(float mouseY, float yTop, float yBottom, RenderingPipeline pipeline)
    {
        float height = yBottom - yTop;

        float t = 1.0f - MathHelper.Clamp(
            (mouseY - yTop) / height,
            0f,
            1f);

        // Logarithmic zoom interpolation.
        float zoom = MinZoom * (float)Math.Pow(
            MaxZoom / MinZoom,
            t);

        pipeline.SetZoom(zoom);
    }
}