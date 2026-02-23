namespace ChangeTrace.Player.Enums;

/// <summary>
/// Represents current playback state of PlaybackTransport.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="Idle"/> – playback has not started or has been reset.</item>
/// <item><see cref="Playing"/> – playback is currently active.</item>
/// <item><see cref="Paused"/> – playback is temporarily halted but can resume.</item>
/// <item><see cref="Finished"/> – playback has reached the end of the timeline.</item>
/// </list>
/// </remarks>
internal enum PlayerState
{
    /// <summary>Playback has not started or has been reset.</summary>
    Idle,

    /// <summary>Playback is currently active.</summary>
    Playing,

    /// <summary>Playback is temporarily halted but can resume.</summary>
    Paused,

    /// <summary>Playback has reached the end of the timeline.</summary>
    Finished
}