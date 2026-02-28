using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.States;

/// <summary>
/// Represents= complete immutable state required to render single frame.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RenderState"/> is frame level snapshot aggregating simulation timing,
/// scene composition, camera configuration, and HUD state.
/// </para>
/// <para>
/// It is intended to be consumed by rendering layer as pure data structure,
/// enabling deterministic rendering and optional interpolation between frames.
/// </para>
/// </remarks>
/// <param name="VirtualTime">
/// Current simulation time in virtual seconds.
/// </param>
/// <param name="WallDelta">
/// Elapsed real world time (in seconds) since previous frame.
/// </param>
/// <param name="Progress">
/// Normalized playback progress in range 0.0–1.0.
/// </param>
/// <param name="CurrentSpeed">
/// Current playback speed multiplier.
/// </param>
/// <param name="Scene">
/// Snapshot of the scene graph, including actors, nodes, and edges.
/// </param>
/// <param name="Camera">
/// Snapshot of camera state used for rendering.
/// </param>
/// <param name="Hud">
/// Snapshot of the HeadsUp Display state.
/// </param>
internal sealed record RenderState(
    double VirtualTime,
    double WallDelta,
    double Progress,
    double CurrentSpeed,
    ISceneSnapshot Scene,
    CameraSnapshot Camera,
    HudState Hud
)
{
    /// <summary>
    /// Gets value indicating whether the scene contains any renderable activity.
    /// </summary>
    /// <remarks>
    /// Returns <c>false</c> when the scene snapshot is empty.
    /// </remarks>
    internal bool HasActivity => !Scene.IsEmpty;

    /// <summary>
    /// Computes the amount of virtual time that has elapsed since a previous timestamp.
    /// </summary>
    /// <param name="previousTime">The prior virtual time value.</param>
    /// <returns>
    /// A non-negative virtual time delta, suitable for interpolation calculations.
    /// </returns>
    /// <remarks>
    /// The result is clamped to zero to prevent negative deltas in case of clock drift
    /// or timeline resets.
    /// </remarks>
    internal double VirtualDelta(double previousTime) =>
        Math.Max(0, VirtualTime - previousTime);
}