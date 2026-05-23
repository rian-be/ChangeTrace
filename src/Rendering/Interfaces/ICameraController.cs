using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Controls camera movement, zoom, and follow behavior.
/// </summary>
internal interface ICameraController
{
    /// <summary>
    /// Current camera follow mode.
    /// </summary>
    CameraFollowMode Mode { get; set; }

    /// <summary>
    /// Optional actor identifier used by follow modes.
    /// </summary>
    string? TargetActorId { get; set; }

    /// <summary>
    /// Updates camera state for the current frame.
    /// </summary>
    void Tick(
        ISceneGraph scene,
        float dt,
        Vec2 viewportSize);

    /// <summary>
    /// Applies camera panning offset.
    /// </summary>
    void Pan(Vec2 delta);

    /// <summary>
    /// Applies camera zoom delta.
    /// </summary>
    void Zoom(float deltaZoom);

    /// <summary>
    /// Sets absolute camera position.
    /// </summary>
    void SetPosition(Vec2 position);
}