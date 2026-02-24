using ChangeTrace.Core;

namespace ChangeTrace.Player.Factory;

internal static class TimelineDurationCalculator
{
    /// <summary>
    /// Calculates playback duration the Gource way:
    /// each day with at least one event gets <paramref name="secondsPerDay"/> of screen time.
    /// </summary>
    /// <param name="timeline">Normalized (offsets removed) or raw timeline.</param>
    /// <param name="secondsPerDay">Screen seconds per active day (Gource default = 10).</param>
    /// <param name="minDuration">Floor duration in seconds.</param>
    /// <param name="maxDuration">Ceiling duration in seconds.</param>
    public static double Calculate(
        Timeline timeline,
        double secondsPerDay = 10.0,
        double minDuration   = 30.0,
        double maxDuration   = 3600.0)
    {
        if (timeline.Count == 0) return minDuration;

        var activeDays = timeline.Events
            .Select(e => DateTimeOffset
                .FromUnixTimeSeconds(e.Timestamp.UnixSeconds)
                .Date)
            .Distinct()
            .Count();

        var duration = activeDays * secondsPerDay;
        return Math.Clamp(duration, minDuration, maxDuration);
    }
}