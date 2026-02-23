using ChangeTrace.Core.Results;
using ChangeTrace.Player.Enums;

namespace ChangeTrace.Player.Interfaces;

/// <summary>
/// Represents  playback transport, responsible for play, pause, and stop operations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Maintains current <see cref="PlayerState"/>.</item>
/// <item>Supports different <see cref="PlaybackMode"/> (Once, Loop, PingPong).</item>
/// <item>Exposes events for state changes and periodic ticks.</item>
/// <item>Implements <see cref="IDisposable"/> to release internal timer or resources.</item>
/// </list>
/// </remarks>
internal interface IPlaybackTransport : IDisposable
{
    /// <summary>Current playback state.</summary>
    PlayerState State { get; }

    /// <summary>Playback mode, e.g., Once, Loop, PingPong.</summary>
    PlaybackMode Mode { get; set; }

    /// <summary>Starts playback.</summary>
    /// <returns>Result indicating success or failure.</returns>
    Result Play();

    /// <summary>Pauses playback.</summary>
    /// <returns>Result indicating success or failure.</returns>
    Result Pause();

    /// <summary>Stops playback.</summary>
    /// <returns>Result indicating success or failure.</returns>
    Result Stop();

    /// <summary>Triggered when <see cref="State"/> changes.</summary>
    event Action<PlayerState>? OnStateChanged;

    /// <summary>Triggered on each playback tick.</summary>
    event Action? OnTick;
}