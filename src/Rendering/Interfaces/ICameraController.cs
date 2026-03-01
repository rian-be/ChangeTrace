using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Defines controller responsible for updating camera's position, zoom, and rotation
/// based on scene content and selected follow mode.
/// </summary>
/// <remarks>
/// Implementations can follow actors, fit all nodes into viewport, or allow free manual control.
/// <para>
/// The <see cref="Tick"/> method is called each frame with current scene and viewport size.
/// Depending on <see cref="Mode"/> and optionally <see cref="TargetActorId"/>, the controller
/// updates the camera's internal position and zoom to produce smooth visualization.
/// </para>
/// </remarks>
internal interface ICameraController
{
    /// <summary>
    /// Gets or sets current camera follow mode.
    /// </summary>
    /// <remarks>
    /// Controls how camera moves automatically:
    /// <list type="bullet">
    /// <item><description><see cref="CameraFollowMode.Free"/> — manual control only.</description></item>
    /// <item><description><see cref="CameraFollowMode.FollowAverage"/> — follows center of mass of active actors.</description></item>
    /// <item><description><see cref="CameraFollowMode.FollowActive"/> — follows most recently active actor.</description></item>
    /// <item><description><see cref="CameraFollowMode.FitAll"/> — auto-zooms to fit all nodes.</description></item>
    /// </list>
    /// </remarks>
    CameraFollowMode Mode { get; set; }

    /// <summary>
    /// Optional identifier of actor to follow when using follow active modes.
    /// </summary>
    string? TargetActorId { get; set; }

    /// <summary>
    /// Updates camera state for current frame.
    /// </summary>
    /// <param name="scene">The scene graph containing nodes, avatars, and edges.</param>
    /// <param name="dt">Time delta in seconds since last update.</param>
    /// <param name="viewportSize">The size of viewport for positioning and auto fit calculations.</param>
    void Tick(ISceneGraph scene, float dt, Vec2 viewportSize);
}