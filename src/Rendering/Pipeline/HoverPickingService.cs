using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Layout.Hive.Core;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Pipeline;

/// <summary>
/// Tracks mouse position and resolves hovered scene nodes or hive pods.
/// </summary>
internal sealed class HoverPickingService(
    ISceneGraph scene,
    ILayoutEngine layout,
    Camera.Camera camera,
    Vec2 viewportSize)
{
    private Vec2 _mouseScreenPos;
    private Vec2 _lastMousePos;

    /// <summary>
    /// Currently hovered scene node, if any.
    /// </summary>
    public SceneNode? HoveredNode { get; private set; }

    /// <summary>
    /// Currently hovered hive pod, if any.
    /// </summary>
    public HoveredPodHud? HoveredPod { get; private set; }

    /// <summary>
    /// Updates mouse position in screen coordinates.
    /// </summary>
    public void UpdateMouse(
        Vec2 screenPos)
    {
        _mouseScreenPos = screenPos;
    }

    /// <summary>
    /// Updates the hover state when the mouse position changes.
    /// </summary>
    public void Tick()
    {
        if (_mouseScreenPos == _lastMousePos)
            return;

        _lastMousePos = _mouseScreenPos;

        var worldMouse = ScreenToWorld(_mouseScreenPos);

        UpdateHoveredNode(worldMouse);
        UpdateHoveredPod(worldMouse);
    }

    /// <summary>
    /// Converts screen coordinates into world coordinates.
    /// </summary>
    private Vec2 ScreenToWorld(
        Vec2 screenPosition)
    {
        float safeZoom = MathF.Max(camera.Zoom, 0.001f);

        float cos = MathF.Cos(camera.Rotation);
        float sin = MathF.Sin(camera.Rotation);

        float dxScreen = (screenPosition.X - viewportSize.X / 2f) / safeZoom;
        float dyScreen = (viewportSize.Y / 2f - screenPosition.Y) / safeZoom;

        return new Vec2(
            dxScreen * cos + dyScreen * sin + camera.Position.X,
            -dxScreen * sin + dyScreen * cos + camera.Position.Y);
    }

    /// <summary>
    /// Finds the closest pickable scene node under the mouse.
    /// </summary>
    private void UpdateHoveredNode(
        Vec2 worldMouse)
    {
        SceneNode? closest = null;
        float minDistSq = float.MaxValue;

        float safeZoom = MathF.Max(camera.Zoom, 0.001f);
        float minPickRadiusWorld = 12f / safeZoom;

        foreach (var n in scene.Nodes.Values)
        {
            var delta = n.Position - worldMouse;
            float distSq = delta.LengthSq;

            float threshold =
                n.Kind == NodeKind.File
                    ? MathF.Max(n.Radius + 4f, minPickRadiusWorld)
                    : MathF.Max(n.Radius + 5f, 10f / safeZoom);

            if (distSq >= threshold * threshold)
                continue;

            if (distSq >= minDistSq)
                continue;

            minDistSq = distSq;
            closest = n;
        }

        HoveredNode = closest;
    }

    /// <summary>
    /// Finds a hovered hive pod and converts it into HUD data.
    /// </summary>
    private void UpdateHoveredPod(
        Vec2 worldMouse)
    {
        HoveredPod = null;

        if (layout is not HiveLayout hiveLayout)
            return;

        float safeZoom = MathF.Max(camera.Zoom, 0.001f);

        var pod = hiveLayout.HitTestCluster(
            worldMouse,
            padding: 48f / safeZoom);

        if (pod == null)
            return;

        HoveredPod = new HoveredPodHud(
            pod.Id,
            pod.Label,
            pod.Center,
            pod.LabelPosition,
            pod.Radius,
            pod.ActivityScore,
            pod.ImportanceScore,
            pod.FileIds);
    }

    /// <summary>
    /// Clears the current hover state.
    /// </summary>
    public void Clear()
    {
        HoveredNode = null;
        HoveredPod = null;
    }
}