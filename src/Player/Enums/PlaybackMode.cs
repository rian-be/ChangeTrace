namespace ChangeTrace.Player.Enums;

/// <summary>
/// Defines how playback behaves when reaching timeline boundaries.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="Once"/> – play a single pass and stop at the end.</item>
/// <item><see cref="Loop"/> – restart from the beginning and loop indefinitely.</item>
/// <item><see cref="PingPong"/> – reverse direction at each end of the timeline.</item>
/// </list>
/// </remarks>
internal enum PlaybackMode
{
    /// <summary>Play once and stop at the end.</summary>
    Once,

    /// <summary>Jump back to start and replay indefinitely.</summary>
    Loop,

    /// <summary>Reverse direction at each end.</summary>
    PingPong
}