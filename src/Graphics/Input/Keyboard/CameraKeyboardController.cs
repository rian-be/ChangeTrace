using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Pipeline;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ChangeTrace.Graphics.Input.Keyboard;

/// <summary>
/// Handles keyboard shortcuts related to camera and layout control.
/// </summary>
internal sealed class CameraKeyboardController
{
    /// <summary>
    /// Processes keyboard input for camera and layout actions.
    /// </summary>
    public bool Handle(KeyboardState keyboard, RenderingPipeline pipeline)
    {
        bool handled = false;

        // Toggle scene layout mode.
        if (keyboard.IsKeyPressed(Keys.F))
        {
            pipeline.ToggleLayoutMode();
            handled = true;
        }

        // Follow the repository average position.
        if (keyboard.IsKeyPressed(Keys.F1))
        {
            pipeline.SetCameraMode(CameraFollowMode.FollowAverage);
            handled = true;
        }

        // Follow active/highlighted activity.
        if (keyboard.IsKeyPressed(Keys.F2))
        {
            pipeline.SetCameraMode(CameraFollowMode.FollowActive);
            handled = true;
        }

        // Fit the entire scene into view.
        if (keyboard.IsKeyPressed(Keys.F3))
        {
            pipeline.SetCameraMode(CameraFollowMode.FitAll);

            handled = true;
        }

        // Enable fully manual/free camera mode.
        if (keyboard.IsKeyPressed(Keys.F4))
        {
            pipeline.SetCameraMode(CameraFollowMode.Free);
            handled = true;
        }

        return handled;
    }
}