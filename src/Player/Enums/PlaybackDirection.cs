namespace ChangeTrace.Player.Enums;

/// <summary>
/// Direction of playback for timeline or event cursor.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="Forward"/> – normal forward playback.</item>
/// <item><see cref="Backward"/> – reverse playback.</item>
/// </list>
/// </remarks>
internal enum PlaybackDirection
{
    /// <summary>Normal forward playback.</summary>
    Forward = +1,

    /// <summary>Reverse/backward playback.</summary>
    Backward = -1
}