using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ChangeTrace.Graphics.Input.Keyboard;

/// <summary>
/// Handles debug related keyboard shortcuts.
/// </summary>
internal sealed class DebugKeyboardController
{
    /// <summary>
    /// Processes debug keyboard input.
    /// </summary>
    public void Handle(KeyboardState keyboard, Action close)
    {
        // Close the application window.
        if (!keyboard.IsKeyPressed(Keys.Escape)) return;

        close();
    }
}