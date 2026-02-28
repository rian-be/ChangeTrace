namespace ChangeTrace.Rendering.Enums;

/// <summary>
/// Defines camera behavior mode in scene.
/// </summary>
internal enum CameraFollowMode
{
    Free,           // manual only
    FollowAverage,  // center of mass of all active actors
    FollowActive,   // follow most recently active actor
    FitAll          // auto-zoom to fit all nodes
}