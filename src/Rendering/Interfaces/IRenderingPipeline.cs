using ChangeTrace.Player.Interfaces;
using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Drives scene rendering from a timeline player.
/// </summary>
internal interface IRenderingPipeline : IDisposable
{
    /// <summary>
    /// Timeline player driving the pipeline.
    /// </summary>
    ITimelinePlayer Player { get; }

    /// <summary>
    /// Starts event processing and rendering updates.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops event processing and rendering updates.
    /// </summary>
    void Stop();

    /// <summary>
    /// Applies camera panning offset.
    /// </summary>
    void PanCamera(Vec2 delta);

    /// <summary>
    /// Applies camera zoom delta.
    /// </summary>
    void ZoomCamera(float deltaZoom);

    /// <summary>
    /// Sets active camera follow mode.
    /// </summary>
    void SetCameraMode(CameraFollowMode mode);

    /// <summary>
    /// Updates current mouse screen position.
    /// </summary>
    void UpdateMouse(Vec2 screenPos);
}