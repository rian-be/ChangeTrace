namespace ChangeTrace.Player.Interfaces;

/// <summary>
/// Represents virtual playback clock for timeline simulation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Exposes wall-clock and virtual time positions.</item>
/// <item>Supports variable playback speed, acceleration, and ramping.</item>
/// <item>Allows external control over speed and position (snap, reanchor, freeze).</item>
/// </list>
/// </remarks>
internal interface IVirtualClock
{
    /// <summary>Current real-world wall time in seconds.</summary>
    double WallNow { get; }

    /// <summary>Current virtual time in seconds within timeline.</summary>
    double VirtualNow { get; }

    /// <summary>Current playback speed multiplier.</summary>
    double CurrentSpeed { get; }

    /// <summary>Target playback speed multiplier.</summary>
    double TargetSpeed { get; }

    /// <summary>Acceleration applied when ramping to target speed.</summary>
    double Acceleration { get; set; }

    /// <summary>Indicates if clock is currently ramping towards target speed.</summary>
    bool IsRamping { get; }

    /// <summary>Starts or resumes the virtual clock.</summary>
    void Start();

    /// <summary>Resets the clock to initial state (0 position, speed).</summary>
    void Reset();

    /// <summary>Sets new target speed; ramping may occur depending on <see cref="Acceleration"/>.</summary>
    /// <param name="target">Target speed multiplier.</param>
    void SetTargetSpeed(double target);

    /// <summary>Instantly snaps playback speed to given value.</summary>
    /// <param name="speed">Speed multiplier.</param>
    void SnapSpeed(double speed);

    /// <summary>Instantly snaps virtual timeline position.</summary>
    /// <param name="virtualPos">Target virtual position in seconds.</param>
    void SnapPosition(double virtualPos);

    /// <summary>Reanchors internal clock reference points (e.g., after a snap).</summary>
    void Reanchor();

    /// <summary>Freezes clock, pausing virtual time progression.</summary>
    void Freeze();
}