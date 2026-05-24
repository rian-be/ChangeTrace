using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Pipeline;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Input.Mouse;

/// <summary>
/// Handles mouse drag camera movement.
/// </summary>
internal sealed class MouseDragController
{
    private bool _dragging;
    private Vector2 _lastMouse;

    /// <summary>
    /// Gets whether the drag operation is currently active.
    /// </summary>
    public bool IsDragging => _dragging;

    /// <summary>
    /// Starts a mouse drag operation.
    /// </summary>
    public void Begin(Vector2 mousePosition)
    {
        _dragging = true;
        _lastMouse = mousePosition;
    }

    /// <summary>
    /// Ends the current mouse drag operation.
    /// </summary>
    public void End() => _dragging = false;

    /// <summary>
    /// Updates camera panning from mouse movement.
    /// </summary>
    public void Update(Vector2 mousePosition, RenderingPipeline pipeline)
    {
        if (!_dragging)
            return;

        Vector2 delta = mousePosition - _lastMouse;
        _lastMouse = mousePosition;

        // Invert X movement to match camera-space dragging.
        var worldDelta = new Vec2(-delta.X, delta.Y);

        pipeline.PanCamera(worldDelta);
    }
}