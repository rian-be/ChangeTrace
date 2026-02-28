using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.Camera;

/// <summary>
/// Represents mutable camera state used by rendering layer.
/// </summary>
/// <remarks>
/// Maintains position, zoom, and rotation.
/// Provides basic zoom anchoring logic and snapshot conversion.
/// </remarks>
internal sealed class Camera
{
    /// <summary>
    /// Camera world position.
    /// </summary>
    internal Vec2 Position { get; set; } = Vec2.Zero;

    /// <summary>
    /// Camera zoom factor.
    /// </summary>
    internal float Zoom { get; set; } = 1f;

    /// <summary>
    /// Camera rotation in radians.
    /// </summary>
    internal float Rotation { get; set; } = 0f;

    /// <summary>
    /// Minimum allowed zoom.
    /// </summary>
    internal const float MinZoom = 0.05f;

    /// <summary>
    /// Maximum allowed zoom.
    /// </summary>
    internal const float MaxZoom = 10f;

    /// <summary>
    /// Applies zoom change relative to given anchor point.
    /// </summary>
    /// <param name="delta">Relative zoom delta.</param>
    /// <param name="anchor">World space anchor position.</param>
    internal void ZoomAt(float delta, Vec2 anchor)
    {
        float newZoom = Math.Clamp(Zoom * (1f + delta), MinZoom, MaxZoom);
        Position += (anchor - Position) * (1f - newZoom / Zoom);
        Zoom = newZoom;
    }

    /// <summary>
    /// Creates immutable snapshot of camera state.
    /// </summary>
    /// <returns><see cref="CameraSnapshot"/> instance.</returns>
    internal CameraSnapshot ToSnapshot()
        => new(Position, Zoom, Rotation);
}