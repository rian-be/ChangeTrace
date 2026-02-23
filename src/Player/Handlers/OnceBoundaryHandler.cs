using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Player.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Player.Handlers;

/// <summary>
/// Boundary handler for <c>Once</c> playback mode.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IBoundaryHandler"/>.</item>
/// <item>Always signals that playback should stop when boundary is reached.</item>
/// <item>Does not trigger any loop or ping-pong behavior.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class OnceBoundaryHandler : IBoundaryHandler
{
    /// <summary>
    /// Handles boundary check for the cursor.
    /// </summary>
    /// <param name="cursor">The event cursor.</param>
    /// <param name="clock">The virtual clock.</param>
    /// <param name="loopFired">Outputs <c>true</c> if a loop/ping-pong occurred (always <c>false</c> here).</param>
    /// <returns><c>true</c> if playback should stop, <c>false</c> otherwise.</returns>
    public bool Handle(IEventCursor cursor, IVirtualClock clock, out bool loopFired)
    {
        loopFired = false;
        return true; // always stop
    }
}