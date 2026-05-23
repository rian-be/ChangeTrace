using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;
using JetBrains.Annotations;

namespace ChangeTrace.Rendering.States;

/// <summary>
/// Immutable frame snapshot consumed by the renderer.
/// </summary>
[UsedImplicitly]
internal sealed record RenderState(
    double VirtualTime,
    double WallDelta,
    ISceneSnapshot Scene,
    CameraSnapshot Camera,
    HudState Hud,
    LayoutMode Mode,
    float ManagedMemoryMb = 0)
{
    /// <summary>
    /// Indicates whether the frame contains visible scene activity.
    /// </summary>
    internal bool HasActivity =>
        !Scene.IsEmpty;

    /// <summary>
    /// Computes elapsed virtual time since previous frame.
    /// </summary>
    internal double VirtualDelta(double previousTime) =>
        Math.Max(
            0,
            VirtualTime - previousTime);
}