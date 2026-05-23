namespace ChangeTrace.Player.Speed;

/// <summary>
/// Controls playback speed using trapezoidal-ramp kinematics.
/// </summary>
/// <remarks>
/// Virtual displacement is the integral of piecewise-linear velocity:
///     ramp phase: Δx = v0·t + ½·sign·a·t² for t ∈ [0, T_ramp]
///     cruise phase: Δx = v_target · (t – T_ramp)
///
/// All ramp state is anchored at transition time, preserving virtual-time
/// continuity across pauses, resumes, and speed changes.
/// </remarks>
internal sealed class SpeedController
{
    internal const double MinSpeed = 0.1;
    internal const double MaxSpeed = 200.0;
    internal const double DefaultSpeed = 1.0;
    internal const double DefaultAccel = 2.0;

    private double _v0;
    private double _rampStartWall;
    private double _virtualAtRampStart;
    private double _rampDuration;

    /// <summary>
    /// Gets current playback speed.
    /// </summary>
    internal double CurrentSpeed { get; private set; }

    /// <summary>
    /// Gets target playback speed.
    /// </summary>
    internal double TargetSpeed { get; private set; }

    /// <summary>
    /// Gets or sets acceleration used for speed ramps.
    /// </summary>
    internal double Acceleration { get; set; } = DefaultAccel;

    /// <summary>
    /// Gets whether playback speed is currently ramping toward the target speed.
    /// </summary>
    internal bool IsRamping => Math.Abs(CurrentSpeed - TargetSpeed) > 1e-9;

    /// <summary>
    /// Creates speed controller with initial playback speed.
    /// </summary>
    /// <param name="initialSpeed">Initial playback speed.</param>
    internal SpeedController(double initialSpeed = DefaultSpeed)
    {
        Validate(initialSpeed);
        CurrentSpeed = TargetSpeed = _v0 = initialSpeed;
        Console.WriteLine($"Speed: {CurrentSpeed}, Target: {TargetSpeed}");
    }

    /// <summary>
    /// Begins smooth speed ramp from current state to target speed.
    /// </summary>
    /// <param name="target">Target playback speed.</param>
    /// <param name="wallNow">Current wall-clock time.</param>
    /// <param name="virtualNow">Current virtual position.</param>
    internal void SetTarget(double target, double wallNow, double virtualNow)
    {
        Validate(target);
        Anchor(wallNow, virtualNow);
        TargetSpeed = target;
        _rampDuration = RampDuration();
    }

    /// <summary>
    /// Re-anchors ramp state after pause or resume without changing the target speed.
    /// </summary>
    /// <param name="wallNow">Current wall-clock time.</param>
    /// <param name="virtualNow">Current virtual position.</param>
    internal void Reanchor(double wallNow, double virtualNow)
        => Anchor(wallNow, virtualNow);

    /// <summary>
    /// Snaps immediately to speed and virtual position without ramping.
    /// </summary>
    /// <param name="wallNow">Current wall-clock time.</param>
    /// <param name="virtualPos">Virtual position to anchor at.</param>
    /// <param name="speed">Playback speed to apply.</param>
    internal void SnapTo(double wallNow, double virtualPos, double speed)
    {
        Validate(speed);
        CurrentSpeed = TargetSpeed = _v0 = speed;
        _rampStartWall = wallNow;
        _virtualAtRampStart = virtualPos;
        _rampDuration = 0;
    }

    /// <summary>
    /// Computes virtual position at a specified wall-clock time.
    /// </summary>
    /// <remarks>
    /// Updates <see cref="CurrentSpeed"/> as side effect.
    /// </remarks>
    /// <param name="wallNow">Wall-clock time to evaluate.</param>
    /// <returns>Virtual position at a specified wall-clock time.</returns>
    internal double VirtualAt(double wallNow)
    {
        var t = wallNow - _rampStartWall;

        if (_rampDuration < 1e-12 || t >= _rampDuration)
        {
            CurrentSpeed = TargetSpeed;
            return _virtualAtRampStart
                + RampDisplacement(_rampDuration)
                + TargetSpeed * Math.Max(0, t - _rampDuration);
        }

        CurrentSpeed = _v0 + Sign() * Acceleration * t;
        return _virtualAtRampStart + RampDisplacement(t);
    }

    /// <summary>
    /// Anchors ramp state at the current wall-clock and virtual position.
    /// </summary>
    /// <param name="wallNow">Current wall-clock time.</param>
    /// <param name="virtualNow">Current virtual position.</param>
    private void Anchor(double wallNow, double virtualNow)
    {
        _v0 = CurrentSpeed;
        _rampStartWall = wallNow;
        _virtualAtRampStart = virtualNow;
        _rampDuration = RampDuration();
    }

    /// <summary>
    /// Computes virtual displacement during ramp phase.
    /// </summary>
    /// <param name="t">Elapsed ramp time.</param>
    /// <returns>Virtual displacement after elapsed ramp time.</returns>
    private double RampDisplacement(double t)
        => _v0 * t + 0.5 * Sign() * Acceleration * t * t;

    /// <summary>
    /// Computes time required to reach the target speed.
    /// </summary>
    /// <returns>Ramp duration in seconds.</returns>
    private double RampDuration()
        => Acceleration > 0 ? Math.Abs(TargetSpeed - _v0) / Acceleration : 0;

    /// <summary>
    /// Gets ramp direction sign.
    /// </summary>
    /// <returns>Ramp direction as -1, 0, or 1.</returns>
    private double Sign() => Math.Sign(TargetSpeed - _v0);

    /// <summary>
    /// Validates playback speed value.
    /// </summary>
    /// <param name="v">Playback speed to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when speed is outside supported range.
    /// </exception>
    private static void Validate(double v)
    {
        if (v != 0.0 && v is < MinSpeed or > MaxSpeed)
        {
            throw new ArgumentOutOfRangeException(
                nameof(v),
                $"Speed must be in [0, {MinSpeed}–{MaxSpeed}].");
        }
    }
}