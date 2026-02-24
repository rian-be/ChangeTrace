using ChangeTrace.Core.Results;

namespace ChangeTrace.Player.Interfaces;

/// <summary>
/// Single event stepping controller for paused timeline.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Moves the playback cursor forward or backward by exactly one event.</item>
/// <item>Updates the associated virtual clock to the event's timestamp.</item>
/// <item>Returns <see cref="Result"/> indicating success or failure.</item>
/// </list>
/// </remarks>
internal interface IStepper
{
    /// <summary>
    /// Advances cursor by one event.
    /// </summary>
    /// <returns>
    /// <see cref="Result.Success"/> if cursor successfully moved forward;
    /// </returns>
    Result StepForward();

    /// <summary>
    /// Moves cursor backward by one event.
    /// </summary>
    /// <returns>
    /// <see cref="Result.Success"/> if cursor successfully moved backward;
    /// </returns>
    Result StepBackward();
}