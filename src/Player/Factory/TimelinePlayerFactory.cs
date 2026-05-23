using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core;
using ChangeTrace.Core.Timelines;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Interfaces;
using ChangeTrace.Player.Playback;
using Microsoft.Extensions.DependencyInjection;
using ChangeTrace.Core.Diagnostics;

namespace ChangeTrace.Player.Factory;

/// <summary>
/// Factory for creating fully wired <see cref="ITimelinePlayer"/> instances.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Builds player components: <see cref="VirtualClock"/>, <see cref="EventCursor"/>, <see cref="SeekableTimeline"/>, <see cref="PlaybackTransport"/>.</item>
/// <item>Configures initial playback mode, speed, and acceleration.</item>
/// <item>Registered as singleton via <see cref="AutoRegisterAttribute"/> for DI.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class TimelinePlayerFactory(IDiagnosticsProvider diagnostics) : ITimelinePlayerFactory
{
    /// <summary>
    /// Creates a new timeline player with all necessary components wired.
    /// </summary>
    /// <param name="timeline">Timeline to play. Cannot be null.</param>
    /// <param name="mode">Playback mode (Once / Loop / PingPong).</param>
    /// <param name="initialSpeed">Initial virtual speed.</param>
    /// <param name="acceleration">Clock acceleration for speed ramping.</param>
    /// <returns>Fully initialized <see cref="ITimelinePlayer"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="timeline"/> is null.</exception>
    public ITimelinePlayer Create(
        Timeline timeline,
        PlaybackMode mode = PlaybackMode.Once,
        double initialSpeed = 1.0,
        double acceleration = 1.0,
        double secondsPerDay = 1000)
    {
        if (timeline == null) throw new ArgumentNullException(nameof(timeline));
        
        var clock = new VirtualClock(initialSpeed, acceleration);
        var cursor = new EventCursor(timeline.Events);
        
        var targetDuration = TimelineDurationCalculator.Calculate(timeline, secondsPerDay);
        var normResult = ChangeTrace.Core.Timelines.TimelineNormalizer.Normalize(timeline, targetDuration);
        if (!normResult.IsSuccess)
            throw new InvalidOperationException($"Timeline normalization failed: {normResult.Error}");

        
        var seekable = new SeekableTimeline(clock, cursor, targetDuration);

        var transport = new PlaybackTransport();
        return new TimelinePlayer(clock, cursor, seekable, transport, diagnostics, mode);
    }
}