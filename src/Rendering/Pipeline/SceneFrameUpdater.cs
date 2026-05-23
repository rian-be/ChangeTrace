using ChangeTrace.Player;
using ChangeTrace.Player.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Processors;

namespace ChangeTrace.Rendering.Pipeline;

/// <summary>
/// Advances scene simulation, layout, actors, and camera each frame.
/// </summary>
internal sealed class SceneFrameUpdater(
    ISceneGraph scene,
    IAnimationSystem anim,
    ILayoutEngine layout,
    ICameraController cameraCtrl,
    ActorDecaySystem actorDecay,
    Vec2 viewportSize)
{
    private double _lastWallTime;

    /// <summary>
    /// Ticks all frame-level scene systems and returns clamped delta time.
    /// </summary>
    public float Tick(
        PlayerDiagnostics diagnostics)
    {
        float dt = CalculateDeltaTime(
            diagnostics);

        StepLayout(
            diagnostics,
            dt);

        anim.Tick(
            dt);

        scene.TickEdges(
            dt,
            decayRate: 1f);

        actorDecay.Tick(
            scene,
            dt);

        cameraCtrl.Tick(
            scene,
            dt,
            viewportSize);

        return dt;
    }

    /// <summary>
    /// Calculates frame delta from player wall time.
    /// </summary>
    private float CalculateDeltaTime(
        PlayerDiagnostics diagnostics)
    {
        double currentWallTime =
            diagnostics.WallElapsedSeconds;

        float dt =
            (float)Math.Max(
                0,
                currentWallTime - _lastWallTime);

        _lastWallTime =
            currentWallTime;

        if (diagnostics.State == PlayerState.Idle ||
            diagnostics.State == PlayerState.Paused)
        {
            return 0f;
        }

        return Math.Min(
            dt,
            0.05f);
    }

    /// <summary>
    /// Runs one or more layout steps adjusted for playback speed.
    /// </summary>
    private void StepLayout(
        PlayerDiagnostics diagnostics,
        float dt)
    {
        if (dt <= 0f)
            return;

        float speedBoost =
            (float)Math.Max(
                1.0,
                Math.Log10(
                    diagnostics.CurrentSpeed + 9.0));

        float totalPhysicsTime =
            dt * speedBoost;

        float targetStepDt =
            1.0f / 144.0f;

        int layoutSteps =
            (int)Math.Ceiling(
                totalPhysicsTime / targetStepDt);

        layoutSteps =
            Math.Clamp(
                layoutSteps,
                1,
                dt > 0.016f ? 2 : 5);

        float actualStepDt =
            totalPhysicsTime / layoutSteps;

        for (int i = 0; i < layoutSteps; i++)
        {
            layout.Step(
                scene.Nodes,
                actualStepDt);
        }
    }

    /// <summary>
    /// Resets wall-time tracking after the scene clears or playback resets.
    /// </summary>
    public void ResetTime() =>
        _lastWallTime = 0;
}