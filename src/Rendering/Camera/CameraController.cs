using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Camera;

/// <summary>
/// Controls <see cref="Camera"/> instance in scene.
/// </summary>
/// <remarks>
/// Supports smooth damping of position and zoom, multiple follow modes,
/// and optional target actor tracking.
/// </remarks>
internal sealed class CameraController : ICameraController
{
    private readonly Camera _camera;

    private Vec2 _positionVelocity = Vec2.Zero;
    private float _zoomVelocity;

    /// <summary>
    /// Current camera follow mode.
    /// </summary>
    public CameraFollowMode Mode { get; set; } = CameraFollowMode.FollowAverage;

    /// <summary>
    /// Smooth time (in seconds) for camera movement.
    /// </summary>
    private float SmoothingTime { get; set; } = 0.3f;

    /// <summary>
    /// Smooth time (in seconds) for zoom adjustments.
    /// </summary>
    private float ZoomSmoothTime { get; set; } = 0.5f;

    /// <summary>
    /// Optional target actor identifier for camera to follow.
    /// </summary>
    public string? TargetActorId { get; set; }

    /// <summary>
    /// Creates new camera controller for given <see cref="Camera"/>.
    /// </summary>
    /// <param name="camera">The camera instance to control.</param>
    internal CameraController(Camera camera)
    {
        _camera = camera;
    }

    /// <summary>
    /// Updates camera position and zoom based on scene and follow mode.
    /// </summary>
    /// <param name="scene">Current scene graph snapshot.</param>
    /// <param name="dt">Delta time since last update (seconds).</param>
    /// <param name="viewportSize">Size of the viewport in pixels.</param>
    public void Tick(ISceneGraph scene, float dt, Vec2 viewportSize)
    {
        var target = Mode switch
        {
            CameraFollowMode.FollowAverage => ComputeCenterOfMass(scene),
            CameraFollowMode.FollowActive => ComputeActiveActor(scene),
            CameraFollowMode.FitAll => FitAll(scene, viewportSize, dt),
            _ => null
        };

        if (target.HasValue)
            SmoothMove(target.Value, dt);
    }

    /// <summary>
    /// Smoothly moves camera toward target position.
    /// </summary>
    /// <param name="target">Target position in scene space.</param>
    /// <param name="dt"></param>
    private void SmoothMove(Vec2 target, float dt)
    {
        _camera.Position = SmoothDampedVec(
            _camera.Position,
            _positionVelocity,
            target,
            SmoothingTime,
            dt,
            out _positionVelocity
        );
    }

    /// <summary>
    /// Smoothly adjusts camera zoom toward target value.
    /// </summary>
    /// <param name="targetZoom">Target zoom level.</param>
    /// <param name="dt">Delta time since last update.</param>
    private void SmoothZoom(float targetZoom, float dt)
    {
        _camera.Zoom = SmoothDamped(
            _camera.Zoom,
            ref _zoomVelocity,
            targetZoom, ZoomSmoothTime,
            dt
        );
    }

    /// <summary>
    /// Scalar smooth damping using critically damped spring formula.
    /// </summary>
    private static float SmoothDamped(float current, ref float velocity, float target, float smoothTime, float dt)
    {
        float omega = 2f / Math.Max(0.0001f, smoothTime);
        float x     = omega * dt;
        float exp   = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        float delta   = current - target;
        float impulse = (velocity + delta * omega) * dt;

        velocity = (velocity - impulse * omega) * exp;
        return target + (delta + impulse) * exp;
    }

    /// <summary>
    /// Vector smooth damping using critically damped spring formula.
    /// </summary>
    private Vec2 SmoothDampedVec(Vec2 current, Vec2 velocity, Vec2 target, float smoothTime, float dt, out Vec2 newVelocity)
    {
        float omega = 2f / Math.Max(0.0001f, smoothTime);
        float x     = omega * dt;
        float exp   = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

        Vec2 delta   = current - target;
        Vec2 impulse = (velocity + delta * omega) * dt;

        newVelocity = (velocity - impulse * omega) * exp;
        return target + (delta + impulse) * exp;
    }

    /// <summary>
    /// Computes camera target and zoom to fit all nodes in viewport.
    /// </summary>
    /// <param name="scene">Scene graph to fit.</param>
    /// <param name="viewportSize">Viewport size in pixels.</param>
    /// <param name="dt">Delta time since last update.</param>
    /// <returns>Target position for camera or <c>null</c> if no nodes.</returns>
    private Vec2? FitAll(ISceneGraph scene, Vec2 viewportSize, float dt)
    {
        if (scene.Nodes.Count == 0)
            return null;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var node in scene.Nodes.Values)
        {
            var p = node.Position;
            minX = Math.Min(minX, p.X);
            maxX = Math.Max(maxX, p.X);
            minY = Math.Min(minY, p.Y);
            maxY = Math.Max(maxY, p.Y);
        }

        const float padding = 200f;
        float rangeX = maxX - minX + padding;
        float rangeY = maxY - minY + padding;

        float targetZoom = Math.Clamp(
            Math.Min(viewportSize.X / rangeX, viewportSize.Y / rangeY),
            Camera.MinZoom,
            Camera.MaxZoom
        );

        SmoothZoom(targetZoom, dt);

        return new Vec2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
    }

    /// <summary>
    /// Computes center of mass of all avatars in scene.
    /// </summary>
    /// <param name="scene">Scene graph snapshot.</param>
    /// <returns>Center position or <c>null</c> if no avatars.</returns>
    private static Vec2? ComputeCenterOfMass(ISceneGraph scene)
    {
        if (scene.Avatars.Count == 0)
            return null;

        Vec2 sum = Vec2.Zero;
        foreach (var avatar in scene.Avatars.Values)
            sum += avatar.Position;

        return sum / scene.Avatars.Count;
    }

    /// <summary>
    /// Computes position of target actor or center of mass if target not set.
    /// </summary>        
    /// <param name="scene">Scene graph snapshot.</param>
    /// <returns>Target position for camera.</returns>
    private Vec2? ComputeActiveActor(ISceneGraph scene)
    {
        if (TargetActorId is null)
            return ComputeCenterOfMass(scene);

        foreach (var (actorName, avatar) in scene.Avatars)
        {
            if (actorName.Value == TargetActorId)
                return avatar.Position;
        }

        return ComputeCenterOfMass(scene);
    }
}