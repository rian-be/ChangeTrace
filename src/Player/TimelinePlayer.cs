using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Factory;
using ChangeTrace.Player.Handlers;
using ChangeTrace.Player.Interfaces;
using ChangeTrace.Player.Playback;
using ChangeTrace.Player.Speed;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Player;

/// <summary>
/// Coordinates timeline playback by combining virtual-clock timing,
/// event cursor draining, and transport state management.
/// </summary>
/// <remarks>
/// Owns high-level playback orchestration while delegating timing, seeking, stepping, and boundary behavior.
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class TimelinePlayer : ITimelinePlayer
{
    private readonly IVirtualClock _clock;
    private readonly IEventCursor _cursor;
    private readonly ISeekable _seekable;
    private readonly IStepper _stepper;
    private readonly IDiagnosticsProvider _diagnostics;

    private IPlaybackTransport Transport { get; }

    private readonly Lock _lock = new();
    private IBoundaryHandler _boundary;

    private PlayerState _state = PlayerState.Idle;
    private PlaybackMode _mode;
    private PlaybackDirection _direction = PlaybackDirection.Forward;

    private int _eventsFired;
    private int _loopCount;
    private int _tickCount;
    private int _totalEventsAcrossTicks;
    private double _lastNonZeroSpeed = 1.0;

    /// <summary>
    /// Gets total timeline duration in seconds.
    /// </summary>
    public double DurationSeconds => _seekable.DurationSeconds;

    /// <summary>
    /// Gets current transport playback state.
    /// </summary>
    public PlayerState State => Transport.State;

    /// <summary>
    /// Gets the current playback direction.
    /// </summary>
    public PlaybackDirection Direction => _direction;

    /// <summary>
    /// Gets or sets playback boundary mode.
    /// </summary>
    public PlaybackMode Mode
    {
        get => _mode;
        set
        {
            lock (_lock)
            {
                _mode = value;
                _boundary = BoundaryHandlerFactory.Create(value);
                Transport.Mode = value;
            }
        }
    }

    /// <summary>
    /// Gets current playback speed.
    /// </summary>
    public double CurrentSpeed => _clock.CurrentSpeed;

    /// <summary>
    /// Gets or sets a target playback speed.
    /// </summary>
    public double TargetSpeed
    {
        get => _clock.TargetSpeed;
        set
        {
            lock (_lock)
                _clock.SetTargetSpeed(value);
        }
    }

    /// <summary>
    /// Gets or sets playback speed acceleration.
    /// </summary>
    public double Acceleration
    {
        get => _clock.Acceleration;
        set
        {
            lock (_lock)
                _clock.Acceleration = value;
        }
    }

    /// <summary>
    /// Gets the current virtual timeline position in seconds.
    /// </summary>
    public double PositionSeconds => _clock.VirtualNow;

    /// <summary>
    /// Gets playback progress as a normalized event-drain ratio.
    /// </summary>
    public double Progress =>
        _cursor.TotalEvents > 0
            ? (double)_eventsFired / _cursor.TotalEvents
            : 0;

    /// <summary>
    /// Raised when a timeline event is emitted.
    /// </summary>
    public event Action<TraceEvent>? OnEvent;

    /// <summary>
    /// Raised when transport playback state changes.
    /// </summary>
    public event Action<PlayerState>? OnStateChanged
    {
        add => Transport.OnStateChanged += value;
        remove => Transport.OnStateChanged -= value;
    }

    /// <summary>
    /// Raised when playback progress changes.
    /// </summary>
    public event Action<double>? OnProgress;

    /// <summary>
    /// Raised when the loop boundary completes.
    /// </summary>
    public event Action<int>? OnLoopCompleted;

    /// <summary>
    /// Creates a timeline player with required playback services.
    /// </summary>
    /// <param name="clock">Virtual clock used for playback timing.</param>
    /// <param name="cursor">Event cursor used to drain timeline events.</param>
    /// <param name="seekable">Seek service used for timeline positioning.</param>
    /// <param name="transport">Playback transport driving ticks and state changes.</param>
    /// <param name="diagnostics">Diagnostics provider used for runtime metrics.</param>
    /// <param name="mode">Initial playback boundary mode.</param>
    internal TimelinePlayer(
        IVirtualClock clock,
        IEventCursor cursor,
        ISeekable seekable,
        IPlaybackTransport transport,
        IDiagnosticsProvider diagnostics,
        PlaybackMode mode = PlaybackMode.Once)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _cursor = cursor ?? throw new ArgumentNullException(nameof(cursor));
        _seekable = seekable ?? throw new ArgumentNullException(nameof(seekable));
        Transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _stepper = new TimelineStepper(_cursor, _clock, OnEvent);

        _mode = mode;
        _boundary = BoundaryHandlerFactory.Create(mode);
        Transport.Mode = mode;
        Transport.OnTick += OnTransportTick;
    }

    /// <summary>
    /// Handles transport tick by draining events, applying boundary behavior, and publishing progress.
    /// </summary>
    private void OnTransportTick()
    {
        IReadOnlyList<TraceEvent>? batch;
        bool loopFired = false;

        lock (_lock)
        {
            if (_cursor.TotalEvents == 0 || Transport.State != PlayerState.Playing)
                return;

            double virtualNow = _clock.VirtualNow;

            batch = _direction == PlaybackDirection.Forward
                ? _cursor.DrainForward(virtualNow)
                : _cursor.DrainBackward(virtualNow);

            _eventsFired += batch.Count;
            _totalEventsAcrossTicks += batch.Count;

            bool boundary = _direction == PlaybackDirection.Forward
                ? _cursor.AtEnd
                : _cursor.AtStart;

            if (boundary)
            {
                bool stop = _boundary.Handle(_cursor, _clock, out loopFired);

                if (_boundary is PingPongBoundaryHandler pp)
                    _direction = pp.Direction;

                if (stop)
                {
                    Transport.Stop();
                    _lastNonZeroSpeed = _clock.TargetSpeed > 0
                        ? _clock.TargetSpeed
                        : _lastNonZeroSpeed;

                    _clock.Freeze();
                    _state = PlayerState.Finished;
                }
                else if (loopFired)
                {
                    _loopCount++;
                }
            }
        }

        foreach (var evt in batch)
            OnEvent?.Invoke(evt);

        OnProgress?.Invoke(Progress);

        if (loopFired)
            OnLoopCompleted?.Invoke(_loopCount);
    }

    /// <summary>
    /// Starts or resumes timeline playback.
    /// </summary>
    /// <returns>Operation result.</returns>
    public Result Play()
    {
        if (_cursor.TotalEvents == 0)
            return Result.Failure("Timeline is empty.");

        lock (_lock)
        {
            _clock.SnapSpeed(_clock.TargetSpeed < 0.001
                ? _lastNonZeroSpeed
                : _clock.TargetSpeed);

            _clock.Start();
            _clock.Reanchor();
        }

        var result = Transport.Play();

        if (result.IsSuccess)
            _state = PlayerState.Playing;

        return result;
    }

    /// <summary>
    /// Pauses timeline playback and freezes virtual clock.
    /// </summary>
    /// <returns>Operation result.</returns>
    public Result Pause()
    {
        var result = Transport.Pause();

        if (result.IsSuccess)
        {
            _lastNonZeroSpeed = _clock.TargetSpeed > 0
                ? _clock.TargetSpeed
                : _lastNonZeroSpeed;

            _clock.Freeze();
            _state = PlayerState.Paused;
        }

        return result;
    }

    /// <summary>
    /// Stops playback and resets timeline state to beginning.
    /// </summary>
    /// <returns>Operation result.</returns>
    public Result Stop()
    {
        _cursor.ResetToStart();
        _direction = PlaybackDirection.Forward;
        _eventsFired = 0;
        _loopCount = 0;
        _tickCount = 0;
        _totalEventsAcrossTicks = 0;

        var result = Transport.Stop();

        if (result.IsSuccess)
        {
            _clock.Reset();
            _state = PlayerState.Idle;
        }

        return result;
    }

    /// <summary>
    /// Seeks timeline to absolute timestamp.
    /// </summary>
    /// <param name="position">Target timestamp.</param>
    /// <returns>Operation result.</returns>
    public Result Seek(Timestamp position) => _seekable.Seek(position);

    /// <summary>
    /// Seeks timeline by relative time delta.
    /// </summary>
    /// <param name="deltaSeconds">Relative seek delta in seconds.</param>
    /// <returns>Operation result.</returns>
    public Result SeekRelative(double deltaSeconds) => _seekable.SeekRelative(deltaSeconds);

    /// <summary>
    /// Emits next timeline event.
    /// </summary>
    /// <returns>Operation result.</returns>
    public Result StepForward() => _stepper.StepForward();

    /// <summary>
    /// Emits previous timeline event.
    /// </summary>
    /// <returns>Operation result.</returns>
    public Result StepBackward() => _stepper.StepBackward();

    /// <summary>
    /// Applies predefined playback speed.
    /// </summary>
    /// <param name="preset">Speed preset to apply.</param>
    /// <returns>Operation result.</returns>
    public Result ApplyPreset(SpeedPreset preset)
    {
        if (!SpeedPresets.TryGet(preset, out var speed))
            return Result.Failure($"Unknown preset: {preset}.");

        lock (_lock)
            _clock.SnapSpeed(speed);

        return Result.Success();
    }

    /// <summary>
    /// Captures current playback and runtime diagnostics snapshot.
    /// </summary>
    /// <returns>Current player diagnostics.</returns>
    public PlayerDiagnostics GetDiagnostics()
    {
        lock (_lock)
        {
            var firstEvent = _cursor.FirstEvent;

            string? dateLabel = null;
            int elapsedDays = 0;

            if (firstEvent != null)
            {
                double scale = 1.0;
                var lastEvent = _cursor.LastEvent;

                if (lastEvent != null)
                {
                    double totalVirtual =
                        lastEvent.Value.TimeForPlayback -
                        firstEvent.Value.TimeForPlayback;

                    double totalPhysical =
                        lastEvent.Value.Core.Timestamp.UnixSeconds -
                        firstEvent.Value.Core.Timestamp.UnixSeconds;

                    if (totalVirtual > 0.001)
                        scale = totalPhysical / totalVirtual;
                }

                long smoothUnix =
                    firstEvent.Value.Core.Timestamp.UnixSeconds +
                    (long)(_clock.VirtualNow * scale);

                var smoothRes = Timestamp.Create(smoothUnix);
                var smoothTimestamp = smoothRes.IsSuccess
                    ? smoothRes.Value
                    : firstEvent.Value.Core.Timestamp;

                dateLabel = smoothTimestamp.ToString();

                var duration = smoothTimestamp.Subtract(firstEvent.Value.Core.Timestamp);
                elapsedDays = (int)(duration.TotalSeconds / 86400.0);
            }

            return new PlayerDiagnostics(
                State: _state,
                Mode: _mode,
                Direction: _direction,
                CurrentSpeed: _clock.CurrentSpeed,
                TargetSpeed: _clock.TargetSpeed,
                IsRamping: _clock.IsRamping,
                PositionSeconds: _clock.VirtualNow,
                DurationSeconds: DurationSeconds,
                Progress: Progress,
                EventsFired: _eventsFired,
                TotalEvents: _cursor.TotalEvents,
                LoopCount: _loopCount,
                WallElapsedSeconds: _clock.WallNow,
                TickCount: _tickCount,
                AvgEventsPerTick: _tickCount > 0
                    ? (double)_totalEventsAcrossTicks / _tickCount
                    : 0,
                CurrentDateLabel: dateLabel,
                ElapsedDays: elapsedDays,
                ManagedMemoryMb: _diagnostics.GetMemoryMetrics().ManagedMb,
                UnmanagedMemoryMb: _diagnostics.GetMemoryMetrics().WorkingSetMb,
                GcCollections: _diagnostics.GetGcCollections(),
                CpuUsage: _diagnostics.GetRuntimeMetrics().CpuUsagePercentage,
                ThreadCount: _diagnostics.GetRuntimeMetrics().ThreadCount,
                UpTime: _diagnostics.GetRuntimeMetrics().UpTimeSeconds);
        }
    }

    /// <summary>
    /// Releases playback transport resources.
    /// </summary>
    public void Dispose() => Transport.Dispose();
}