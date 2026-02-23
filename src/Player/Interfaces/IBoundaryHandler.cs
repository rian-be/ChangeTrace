namespace ChangeTrace.Player.Interfaces;

/// <summary>
/// Handles playback boundaries such as loops, start/end, or ping-pong behavior.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Determines if playback should stop or continue based on cursor and virtual clock.</item>
/// <item>Notifies when a loop or ping-pong boundary has fired via loopFired</item>
/// <item>Decoupled from playback timing; works with <see cref="IEventCursor"/> and <see cref="IVirtualClock"/>.</item>
/// </list>
/// </remarks>
internal interface IBoundaryHandler
{
    /// <summary>
    /// Handles boundary logic at current playback position.
    /// </summary>
    /// <param name="cursor">Current event cursor.</param>
    /// <param name="clock">Virtual clock to evaluate time-based conditions.</param>
    /// <param name="loopFired">Output flag indicating a loop or ping-pong turn occurred.</param>
    /// <returns>
    /// <c>true</c> if playback should stop; <c>false</c> if it continues.
    /// </returns>
    bool Handle(IEventCursor cursor, IVirtualClock clock, out bool loopFired);
}