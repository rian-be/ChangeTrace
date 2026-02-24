namespace ChangeTrace.Player.Speed;

/// <summary>
/// SRP: owns only the trapezoidal-ramp kinematics.
///
/// Virtual displacement = integral of piecewise-linear v(t):
///   ramp  phase : Δx = v0·t + ½·sign·a·t²   for t ∈ [0, T_ramp]
///   cruise phase : Δx = v_target · (t – T_ramp)
///
/// All state is anchored at the moment a ramp begins, so clock is
/// always continuous regardless of how many speed changes occur.
/// </summary>
internal sealed class SpeedController
{
    internal const double MinSpeed      = 0.1;
    internal const double MaxSpeed      = 200.0;
    internal const double DefaultSpeed  = 1.0;
    internal const double DefaultAccel  = 2.0;

    // Ramp anchor (updated on every SetTarget / SnapTo)
    private double _v0;
    private double _rampStartWall;
    private double _virtualAtRampStart;
    private double _rampDuration;   

    internal double CurrentSpeed { get; private set; }
    internal double TargetSpeed  { get; private set; }
    internal double Acceleration { get; set; } = DefaultAccel;
    internal bool   IsRamping    => Math.Abs(CurrentSpeed - TargetSpeed) > 1e-9;

    internal SpeedController(double initialSpeed = DefaultSpeed)
    {
        Validate(initialSpeed);
        CurrentSpeed = TargetSpeed = _v0 = initialSpeed;
        Console.WriteLine($"Speed: {CurrentSpeed}, Target: {TargetSpeed}");
    }
    
    // Mutators
    /// <summary>Begin smooth ramp to <paramref name="target"/> from current state.</summary>
    internal void SetTarget(double target, double wallNow, double virtualNow)
    {
        Validate(target);
        Anchor(wallNow, virtualNow);
        TargetSpeed   = target;
        _rampDuration = RampDuration();
    }

    /// <summary>Re-anchor after a pause/resume without changing target.</summary>
    internal void Reanchor(double wallNow, double virtualNow)
        => Anchor(wallNow, virtualNow);

    /// <summary>Instant snap — no ramp. Used by presets and seek.</summary>
    internal void SnapTo(double wallNow, double virtualPos, double speed)
    {
        Validate(speed);
        CurrentSpeed = TargetSpeed = _v0 = speed;
        _rampStartWall      = wallNow;
        _virtualAtRampStart = virtualPos;
        _rampDuration       = 0;
    }
    
    // Query
    /// <summary>
    /// Virtual position at wall-clock time <paramref name="wallNow"/>.
    /// Updates <see cref="CurrentSpeed"/> as side effect.
    /// </summary>
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

    private void Anchor(double wallNow, double virtualNow)
    {
        _v0                 = CurrentSpeed;
        _rampStartWall      = wallNow;
        _virtualAtRampStart = virtualNow;
        _rampDuration       = RampDuration();
    }

    private double RampDisplacement(double t)
        => _v0 * t + 0.5 * Sign() * Acceleration * t * t;

    private double RampDuration()
        => Acceleration > 0 ? Math.Abs(TargetSpeed - _v0) / Acceleration : 0;

    private double Sign() => Math.Sign(TargetSpeed - _v0);

    private static void Validate(double v)
    {
        if (v is < MinSpeed or > MaxSpeed)
            throw new ArgumentOutOfRangeException(nameof(v),
                $"Speed must be in [{MinSpeed}–{MaxSpeed}].");
    }
}