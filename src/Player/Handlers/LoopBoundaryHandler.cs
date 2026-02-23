using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Player.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Player.Handlers;

/// <summary>
/// Boundary handler for <c>Loop</c> playback mode.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IBoundaryHandler"/>.</item>
/// <item>Resets the cursor and clock to the start when timeline end is reached.</item>
/// <item>Signals that playback should continue indefinitely.</item>
/// <item>Sets <c>loopFired</c> to <c>true</c> to indicate a loop occurred.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class LoopBoundaryHandler : IBoundaryHandler
{
    /// <summary>
    /// Handles boundary check for the cursor.
    /// </summary>
    /// <param name="cursor">The event cursor.</param>
    /// <param name="clock">The virtual clock.</param>
    /// <param name="loopFired">Outputs <c>true</c> because a loop occurred.</param>
    /// <returns><c>false</c> to indicate playback should continue.</returns>
    public bool Handle(IEventCursor cursor, IVirtualClock clock, out bool loopFired)
    {
        cursor.ResetToStart();
        clock.SnapPosition(0);
        loopFired = true;
        return false; // continue playback
    }
}