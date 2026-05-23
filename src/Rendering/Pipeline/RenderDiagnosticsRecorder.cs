using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Pipeline;

/// <summary>
/// Records runtime rendering and scene diagnostics.
/// </summary>
internal sealed class RenderDiagnosticsRecorder(
    IDiagnosticsProvider diagnostics)
{
    /// <summary>
    /// Records frame-level rendering, scene, camera, and layout metrics.
    /// </summary>
    public void Record(
        ISceneGraph scene,
        IAnimationSystem animation,
        ILayoutEngine layout,
        Camera.Camera camera,
        float deltaTime)
    {
        diagnostics.RecordMetric(
            "Scene.Nodes",
            scene.Nodes.Count);

        diagnostics.RecordMetric(
            "Scene.Edges",
            scene.Edges.Count);

        diagnostics.RecordMetric(
            "Scene.Particles",
            animation.ParticleCount);

        RecordAvatarMetrics(
            scene);

        diagnostics.RecordMetric(
            "Scene.WallDelta",
            deltaTime);

        diagnostics.RecordMetric(
            "Cam.Zoom",
            camera.Zoom);

        diagnostics.RecordMetric(
            "Layout.Energy",
            layout.Energy);
    }

    /// <summary>
    /// Records aggregate and sample avatar diagnostics.
    /// </summary>
    private void RecordAvatarMetrics(
        ISceneGraph scene)
    {
        double activitySum = 0;

        foreach (var avatar in scene.Avatars.Values)
            activitySum += avatar.ActivityLevel;

        double avgActivity =
            scene.Avatars.Count > 0
                ? activitySum / scene.Avatars.Count
                : 0.0;

        diagnostics.RecordMetric(
            "Scene.Avatars",
            scene.Avatars.Count);

        diagnostics.RecordMetric(
            "Scene.Avatars.AvgActivity",
            avgActivity);

        if (scene.Avatars.Count == 0)
            return;

        var sample =
            scene.Avatars.Values.First();

        diagnostics.RecordEvent(
            "Scene.AvatarSample",
            sample.Actor.Value);

        diagnostics.RecordMetric(
            "Scene.Avatar.X",
            sample.Position.X);

        diagnostics.RecordMetric(
            "Scene.Avatar.Y",
            sample.Position.Y);
    }
}