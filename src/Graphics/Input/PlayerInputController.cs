using ChangeTrace.Graphics.Input.Keyboard;
using ChangeTrace.Graphics.Input.Mouse;
using ChangeTrace.Player;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Pipeline;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ChangeTrace.Graphics.Input;

/// <summary>
/// Coordinates keyboard and mouse input for playback, camera, and debug controls.
/// </summary>
internal sealed class PlayerInputController
{
    private readonly PlaybackKeyboardController _playbackKeyboard = new();
    private readonly CameraKeyboardController _cameraKeyboard = new();
    private readonly DebugKeyboardController _debugKeyboard = new();

    private readonly MouseDragController _mouseDrag = new();
    private readonly MouseZoomController _mouseZoom = new();
    private readonly SliderZoomController _sliderZoom = new();

    /// <summary>
    /// Handles keyboard input for playback, camera and debug actions.
    /// </summary>
    public void HandleKeyboard(KeyboardState keyboard, TimelinePlayer player, RenderingPipeline pipeline, Action close)
    {
        _playbackKeyboard.Handle(keyboard, player);
        _cameraKeyboard.Handle(keyboard, pipeline);
        _debugKeyboard.Handle(keyboard, close);
    }

    /// <summary>
    /// Handles mouse wheel zoom input.
    /// </summary>
    public void HandleMouseWheel(MouseWheelEventArgs e, RenderingPipeline pipeline) =>
        _mouseZoom.Zoom(e.OffsetY, pipeline);

    /// <summary>
    /// Handles left mouse press for slider drag or camera drag.
    /// </summary>
    public void HandleMouseDown(MouseButtonEventArgs e, Vector2 mousePosition, Vector2i viewport, RenderingPipeline pipeline)
    {
        if (e.Button != MouseButton.Left)
            return;

        if (_sliderZoom.TryBegin(mousePosition, viewport, pipeline))
        {
            return;
        }

        _mouseDrag.Begin(mousePosition);
    }

    /// <summary>
    /// Ends active mouse interactions.
    /// </summary>
    public void HandleMouseUp(MouseButtonEventArgs e)
    {
        if (e.Button != MouseButton.Left)
            return;

        _mouseDrag.End();
        _sliderZoom.End();
    }

    /// <summary>
    /// Updates mouse position, slider zoom, and camera drag state.
    /// </summary>
    public void HandleMouseMove(Vector2 mousePosition, Vector2i viewport, RenderingPipeline pipeline)
    {
        pipeline.UpdateMouse(new Vec2(mousePosition.X, mousePosition.Y));
        _sliderZoom.Update(mousePosition, viewport, pipeline);
        _mouseDrag.Update(mousePosition, pipeline);
    }
}