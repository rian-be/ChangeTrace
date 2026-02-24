using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Player.Enums;

namespace ChangeTrace.Player.Interfaces;

internal interface ITimelinePlayer : IDisposable
{
    // ------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------
    PlayerState       State     { get; }
    PlaybackMode      Mode      { get; set; }
    PlaybackDirection Direction { get; }

    // ------------------------------------------------------------------
    // Speed control
    // ------------------------------------------------------------------

    /// <summary>Instantaneous current speed (may differ from Target while ramping).</summary>
    double CurrentSpeed { get; }

    /// <summary>
    /// Desired cruise speed.
    /// Setting this starts a smooth ramp governed by <see cref="Acceleration"/>.
    /// </summary>
    double TargetSpeed  { get; set; }

    /// <summary>Rate of speed change in speed-units per second².</summary>
    double Acceleration { get; set; }

    // ------------------------------------------------------------------
    // Position
    // ------------------------------------------------------------------

    /// <summary>Current virtual position in timeline seconds.</summary>
    double PositionSeconds { get; }

    /// <summary>Total timeline duration in seconds.</summary>
    double DurationSeconds { get; }

    /// <summary>Normalised progress 0.0–1.0.</summary>
    double Progress { get; }

    // ------------------------------------------------------------------
    // Transport commands
    // ------------------------------------------------------------------
    Result Play();
    Result Pause();
    Result Stop();

    /// <summary>Seek to an absolute virtual timestamp.</summary>
    Result Seek(Timestamp position);

    /// <summary>Seek by a relative offset in virtual seconds (negative = rewind).</summary>
    Result SeekRelative(double deltaSeconds);

    // ------------------------------------------------------------------
    // Frame stepping
    // ------------------------------------------------------------------

    /// <summary>Advance exactly one event (works while paused).</summary>
    Result StepForward();

    /// <summary>Rewind to re-emit the previous event (works while paused).</summary>
    Result StepBackward();

    // ------------------------------------------------------------------
    // Speed presets  (instant, no ramping)
    // ------------------------------------------------------------------

    /// <summary>Apply a named speed preset immediately.</summary>
    Result ApplyPreset(SpeedPreset preset);

    // ------------------------------------------------------------------
    // Diagnostics
    // ------------------------------------------------------------------
    PlayerDiagnostics GetDiagnostics();

    // ------------------------------------------------------------------
    // Events
    // ------------------------------------------------------------------

    /// <summary>Fired for every event that passes through the virtual clock.</summary>
    event Action<TraceEvent>? OnEvent;

    /// <summary>Fired when <see cref="State"/> changes.</summary>
    event Action<PlayerState>? OnStateChanged;

    /// <summary>Fired each tick with current normalised progress (0.0–1.0).</summary>
    event Action<double>? OnProgress;

    /// <summary>Fired when a loop/ping-pong boundary is crossed.</summary>
    event Action<int>? OnLoopCompleted;
}