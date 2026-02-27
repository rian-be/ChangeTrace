namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot of camera's state at given frame.
/// </summary>
/// <remarks>
/// Captures position, zoom, and rotation for rendering and coordinate transformations.
/// Provides helper method <see cref="WorldToScreen"/> to convert world-space coordinates
/// to screen-space using the camera transform and a screen center point.
/// </remarks>
internal sealed record CameraSnapshot(
    Vec2  Position,
    float Zoom,
    float Rotation
)
{
    /// <summary>
    /// Transforms world space position into screen space based on camera position, rotation, and zoom.
    /// </summary>
    /// <param name="worldPos">The position in world coordinates.</param>
    /// <param name="screenCenter">The screen-space center point to offset the transformed position.</param>
    /// <returns>Screen-space coordinates of the world position.</returns>
    internal Vec2 WorldToScreen(Vec2 worldPos, Vec2 screenCenter)
    {
        var delta = worldPos - Position;
        float cos = MathF.Cos(-Rotation);
        float sin = MathF.Sin(-Rotation);
        var rotated = new Vec2(delta.X * cos - delta.Y * sin, delta.X * sin + delta.Y * cos);
        return screenCenter + rotated * Zoom;
    }
}