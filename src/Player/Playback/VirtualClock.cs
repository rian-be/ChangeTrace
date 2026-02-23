using System.Diagnostics;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Player.Interfaces;
using ChangeTrace.Player.Speed;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Player.Playback;

/// <summary>
/// Wall clock with virtual time computation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks real time with <see cref="Stopwatch"/>.</item>
/// <item>Delegates kinematics to <see cref="SpeedController"/>.</item>
/// <item>Single source of truth for virtual timeline.</item>
/// <item>Supports starting, resetting, freezing, and reanchoring time.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class VirtualClock : IVirtualClock
{
    private readonly Stopwatch _wall = new();
    private readonly SpeedController _speed;

    /// <summary>Elapsed wall-clock time in seconds.</summary>
    public double WallNow => _wall.Elapsed.TotalSeconds;

    /// <summary>Current virtual time in seconds, computed by <see cref="SpeedController"/>.</summary>
    public double VirtualNow => _speed.VirtualAt(WallNow);

    /// <summary>Current virtual speed in units/second.</summary>
    public double CurrentSpeed => _speed.CurrentSpeed;

    /// <summary>Target speed the clock is ramping towards.</summary>
    public double TargetSpeed => _speed.TargetSpeed;

    /// <summary>Acceleration applied during speed ramping.</summary>
    public double Acceleration
    {
        get => _speed.Acceleration;
        set => _speed.Acceleration = value;
    }

    /// <summary>Indicates whether speed ramp is in progress.</summary>
    public bool IsRamping => _speed.IsRamping;

    /// <summary>
    /// Initializes new virtual clock with given initial speed and acceleration.
    /// </summary>
    /// <param name="initialSpeed">Initial speed in units/second.</param>
    /// <param name="acceleration">Acceleration applied during speed changes.</param>
    internal VirtualClock(double initialSpeed, double acceleration) =>
        _speed = new SpeedController(initialSpeed) { Acceleration = acceleration };

    /// <summary>Starts the wall clock if not already running.</summary>
    public void Start()
    {
        if (!_wall.IsRunning)
            _wall.Start();
    }

    /// <summary>Resets the wall clock and snaps virtual time to zero.</summary>
    public void Reset()
    {
        _wall.Reset();
        _speed.SnapTo(0, 0, _speed.TargetSpeed);
    }

    /// <summary>Sets a new target speed and begins ramping toward it.</summary>
    /// <param name="target">Target speed.</param>
    public void SetTargetSpeed(double target)
        => _speed.SetTarget(target, WallNow, VirtualNow);

    /// <summary>Immediately snaps current speed to a given value.</summary>
    /// <param name="speed">New current speed.</param>
    public void SnapSpeed(double speed)
        => _speed.SnapTo(WallNow, VirtualNow, speed);

    /// <summary>Immediately snaps virtual position to a given value, keeping current speed.</summary>
    /// <param name="virtualPos">Target virtual position.</param>
    public void SnapPosition(double virtualPos)
        => _speed.SnapTo(WallNow, virtualPos, _speed.CurrentSpeed);

    /// <summary>Reanchors current speed and position as a reference for future ramps.</summary>
    public void Reanchor()
        => _speed.Reanchor(WallNow, VirtualNow);

    /// <summary>Freezes the virtual clock at current position and speed.</summary>
    public void Freeze()
        => _speed.SnapTo(WallNow, VirtualNow, _speed.CurrentSpeed);
}