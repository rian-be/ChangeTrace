using ChangeTrace.Configuration.Discovery;
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

[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class TimelinePlayer : ITimelinePlayer
{
    private readonly IVirtualClock _clock;
    private readonly IEventCursor _cursor;
    private readonly ISeekable _seekable;
    private readonly IStepper _stepper;
    
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

    public double DurationSeconds => _seekable.DurationSeconds;
    public PlayerState State => Transport.State;
    public PlaybackDirection Direction => _direction;

    public PlaybackMode Mode
    {
        get => _mode;
        set { lock (_lock) { _mode = value; _boundary = BoundaryHandlerFactory.Create(value); Transport.Mode = value; } }
    }

    public double CurrentSpeed => _clock.CurrentSpeed;
    public double TargetSpeed { get => _clock.TargetSpeed; set { lock (_lock) _clock.SetTargetSpeed(value); } }
    public double Acceleration { get => _clock.Acceleration; set { lock (_lock) _clock.Acceleration = value; } }
    public double PositionSeconds => _clock.VirtualNow;
    public double Progress => DurationSeconds > 1e-9 ? Math.Clamp(_clock.VirtualNow / DurationSeconds, 0, 1) : 0;

    public event Action<TraceEvent>? OnEvent;
    public event Action<PlayerState>? OnStateChanged
    {
        add => Transport.OnStateChanged += value;
        remove => Transport.OnStateChanged -= value;
    }
    public event Action<double>? OnProgress;
    public event Action<int>? OnLoopCompleted;

    internal TimelinePlayer(
        IVirtualClock clock,
        IEventCursor cursor,
        ISeekable seekable,
        IPlaybackTransport transport,
        PlaybackMode mode = PlaybackMode.Once)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _cursor = cursor ?? throw new ArgumentNullException(nameof(cursor));
        _seekable = seekable ?? throw new ArgumentNullException(nameof(seekable));
        Transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _stepper = new TimelineStepper(_cursor, _clock, OnEvent);
        
        _mode = mode;
        _boundary = BoundaryHandlerFactory.Create(mode);
        Transport.Mode = mode;
        Transport.OnTick += OnTransportTick;
    }

    private void OnTransportTick()
    {
        IReadOnlyList<TraceEvent>? batch;
        bool loopFired = false;

        lock (_lock)
        {
            if (_cursor.TotalEvents == 0 || Transport.State != PlayerState.Playing) 
            {
                return;
            }

            double virtualNow = _clock.VirtualNow;

            batch = _direction == PlaybackDirection.Forward
                ? _cursor.DrainForward(virtualNow)
                : _cursor.DrainBackward(virtualNow);

            _eventsFired += batch.Count;
            _totalEventsAcrossTicks += batch.Count;

            bool boundary = _direction == PlaybackDirection.Forward ? _cursor.AtEnd : _cursor.AtStart;

           // Console.WriteLine($"Tick #{_tickCount++}: VirtualNow={virtualNow:F2}, Batch={batch.Count}, TotalEventsFired={_eventsFired}, Direction={_direction}, Boundary={boundary}");

            if (boundary)
            {
                bool stop = _boundary.Handle(_cursor, _clock, out loopFired);
                if (_boundary is PingPongBoundaryHandler pp)
                    _direction = pp.Direction;

                Console.WriteLine($"Boundary hit: Stop={stop}, LoopFired={loopFired}, NewDirection={_direction}");

                if (stop)
                {
                    Transport.Stop();
                    _state = PlayerState.Finished;
                    Console.WriteLine("Transport stopped: finished playback.");
                }
                else if (loopFired)
                {
                    _loopCount++;
                    Console.WriteLine($"Loop completed. Total loops={_loopCount}");
                }
            }
        }

        foreach (var evt in batch)
            OnEvent?.Invoke(evt);

        OnProgress?.Invoke(Progress);

        if (loopFired)
            OnLoopCompleted?.Invoke(_loopCount);
    }

    public Result Play()
    {
        if (_cursor.TotalEvents == 0) return Result.Failure("Timeline is empty.");
        _clock.Start();
        _clock.Reanchor();
        return Transport.Play();
    }

    public Result Pause() => Transport.Pause();

    public Result Stop()
    {
        _cursor.ResetToStart();
        _direction = PlaybackDirection.Forward;
        _eventsFired = 0;
        _loopCount = 0;
        _tickCount = 0;
        _totalEventsAcrossTicks = 0;
        return Transport.Stop();
    }

    public Result Seek(Timestamp position) => _seekable.Seek(position);
    public Result SeekRelative(double deltaSeconds) => _seekable.SeekRelative(deltaSeconds);

    public Result StepForward() => _stepper.StepForward();
    public Result StepBackward() => _stepper.StepBackward();
    
    public Result ApplyPreset(SpeedPreset preset)
    {
        if (!SpeedPresets.TryGet(preset, out var speed))
            return Result.Failure($"Unknown preset: {preset}.");

        lock (_lock) _clock.SnapSpeed(speed);
        return Result.Success();
    }

    public PlayerDiagnostics GetDiagnostics()
    {
        lock (_lock)
        {
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
                AvgEventsPerTick: _tickCount > 0 ? (double)_totalEventsAcrossTicks / _tickCount : 0
            );
        }
    }

    public void Dispose() => Transport.Dispose();
}