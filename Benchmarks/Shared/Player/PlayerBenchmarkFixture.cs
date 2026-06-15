using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Info;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.Player;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Factory;
using ChangeTrace.Player.Interfaces;
using ChangeTrace.Player.Playback;

namespace ChangeTrace.Benchmarks.Shared.Player;

/// <summary>
/// Shared deterministic fixture for player benchmarks.
/// </summary>
/// <remarks>
/// Creates synthetic timelines and player components without starting the timer-driven
/// playback transport, keeping benchmarks deterministic and CPU-bound.
/// </remarks>
internal sealed class PlayerBenchmarkFixture
{
    private readonly IReadOnlyList<TraceEvent> _events;

    private PlayerBenchmarkFixture(
        int eventCount,
        Timeline timeline,
        IReadOnlyList<TraceEvent> events)
    {
        EventCount = eventCount;
        Timeline = timeline;
        _events = events;
    }

    /// <summary>
    /// Number of synthetic timeline events represented by the fixture.
    /// </summary>
    public int EventCount { get; }

    /// <summary>
    /// Synthetic timeline used by factory-level player benchmarks.
    /// </summary>
    public Timeline Timeline { get; }

    /// <summary>
    /// Builds a player benchmark fixture with normalized event playback times.
    /// </summary>
    /// <param name="eventCount">Number of synthetic timeline events to create.</param>
    public static PlayerBenchmarkFixture Create(int eventCount)
    {
        var timeline = CreateTimeline(eventCount);
        TimelineNormalizer.Normalize(timeline, targetDurationSeconds: eventCount);
        return new PlayerBenchmarkFixture(eventCount, timeline, timeline.Events.ToArray());
    }

    /// <summary>
    /// Creates a fresh event cursor over the fixture events.
    /// </summary>
    public EventCursor CreateCursor()
        => new(_events);

    /// <summary>
    /// Creates a fresh stepper using a new cursor and virtual clock.
    /// </summary>
    public TimelineStepper CreateStepper()
        => new(CreateCursor(), CreateClock());

    /// <summary>
    /// Creates a seekable timeline over a new cursor and virtual clock.
    /// </summary>
    public SeekableTimeline CreateSeekable()
        => new(CreateClock(), CreateCursor(), EventCount);

    /// <summary>
    /// Creates a player factory with no-op diagnostics.
    /// </summary>
    public TimelinePlayerFactory CreatePlayerFactory()
        => new(new NoopDiagnosticsProvider());

    /// <summary>
    /// Creates a player harness with manual transport ticking for deterministic player benchmarks.
    /// </summary>
    public PlayerBenchmarkHarness CreateTimelinePlayerHarness(PlaybackMode mode = PlaybackMode.Once)
    {
        var clock = CreateClock();
        var cursor = CreateCursor();
        var seekable = new SeekableTimeline(clock, cursor, EventCount);
        var transport = new ManualPlaybackTransport { Mode = mode };
        var player = new TimelinePlayer(clock, cursor, seekable, transport, new NoopDiagnosticsProvider(), mode);
        return new PlayerBenchmarkHarness(player, clock, transport);
    }

    /// <summary>
    /// Creates a raw synthetic timeline.
    /// </summary>
    /// <param name="eventCount">Number of events to create.</param>
    public static Timeline CreateTimeline(int eventCount)
    {
        var repository = RepositoryId.Create("bench", "player").Value;
        var timeline = new Timeline(repository);
        var actor = ActorName.Create("benchmark-user").Value;
        const long baseUnix = 1_700_000_000;

        for (var i = 0; i < eventCount; i++)
        {
            var timestamp = Timestamp.Create(baseUnix + i).Value;
            timeline.AddEvent(new TraceEvent(
                new TraceEventCore(
                    timestamp,
                    actor,
                    $"src/module-{i % 128}/file-{i}.cs")));
        }

        return timeline;
    }

    /// <summary>
    /// Creates a fresh virtual clock for player and playback benchmarks.
    /// </summary>
    public static VirtualClock CreateClock()
        => new(initialSpeed: 1.0, acceleration: 1.0);

    internal sealed class PlayerBenchmarkHarness : IDisposable
    {
        private int _eventsObserved;

        public PlayerBenchmarkHarness(
            TimelinePlayer player,
            VirtualClock clock,
            ManualPlaybackTransport transport)
        {
            Player = player;
            Clock = clock;
            Transport = transport;
            Player.OnEvent += _ => _eventsObserved++;
        }

        public TimelinePlayer Player { get; }

        public VirtualClock Clock { get; }

        public ManualPlaybackTransport Transport { get; }

        public int EventsObserved => _eventsObserved;

        public void Dispose()
            => Player.Dispose();
    }

    internal sealed class ManualPlaybackTransport : IPlaybackTransport
    {
        public PlayerState State { get; private set; } = PlayerState.Idle;

        public PlaybackMode Mode { get; set; } = PlaybackMode.Once;

        public event Action<PlayerState>? OnStateChanged;

        public event Action? OnTick;

        public Result Play()
        {
            State = PlayerState.Playing;
            OnStateChanged?.Invoke(State);
            return Result.Success();
        }

        public Result Pause()
        {
            if (State != PlayerState.Playing)
                return Result.Failure($"Cannot pause while {State}.");

            State = PlayerState.Paused;
            OnStateChanged?.Invoke(State);
            return Result.Success();
        }

        public Result Stop()
        {
            State = PlayerState.Idle;
            OnStateChanged?.Invoke(State);
            return Result.Success();
        }

        public void TriggerTick()
            => OnTick?.Invoke();

        public void Dispose()
        {
        }
    }

    private sealed class NoopDiagnosticsProvider : IDiagnosticsProvider
    {
        /// <inheritdoc />
        public MemoryMetrics GetMemoryMetrics()
            => new(0, 0, 0);

        /// <inheritdoc />
        public int[] GetGcCollections()
            => [0, 0, 0];

        /// <inheritdoc />
        public RuntimeMetrics GetRuntimeMetrics()
            => new(0, 0, 0, 0);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, double> GetCustomMetrics()
            => new Dictionary<string, double>();

        /// <inheritdoc />
        public void RecordMetric(string key, double value)
        {
        }

        /// <inheritdoc />
        public void RecordEvent(string category, string label)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<KeyValuePair<string, int>> GetTopEvents(string category, int count)
            => [];
    }
}
