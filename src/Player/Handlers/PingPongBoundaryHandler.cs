using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Player.Handlers;

/// <summary>
/// Boundary handler for <c>PingPong</c> playback mode.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IBoundaryHandler"/>.</item>
/// <item>Reverses playback direction at each end of timeline.</item>
/// <item>Signals that playback should continue indefinitely.</item>
/// <item>Sets <c>loopFired</c> to <c>true</c> whenever a direction flip occurs.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class PingPongBoundaryHandler : IBoundaryHandler
{
    private PlaybackDirection _direction = PlaybackDirection.Forward;

    /// <summary>
    /// Current playback direction.
    /// </summary>
    internal PlaybackDirection Direction => _direction;

    /// <summary>
    /// Handles boundary check for the cursor.
    /// </summary>
    /// <param name="cursor">The event cursor.</param>
    /// <param name="clock">The virtual clock.</param>
    /// <param name="loopFired">
    /// Outputs <c>true</c> because a ping-pong turn occurred.
    /// </param>
    /// <returns><c>false</c> to indicate playback should continue.</returns>
    public bool Handle(IEventCursor cursor, IVirtualClock clock, out bool loopFired)
    {
        _direction = _direction == PlaybackDirection.Forward
            ? PlaybackDirection.Backward
            : PlaybackDirection.Forward;

        loopFired = true;
        return false;  // keep playing
    }

    /// <summary>
    /// Resets the playback direction to forward.
    /// </summary>
    internal void Reset() => _direction = PlaybackDirection.Forward;
}