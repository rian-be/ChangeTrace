using ChangeTrace.Core.Results;
using ChangeTrace.Player.Enums;

namespace ChangeTrace.Player.Interfaces;

/// <summary>Speed control with ramping kinematics.</summary>
internal interface ISpeedControl
{
    double CurrentSpeed { get; }
    double TargetSpeed  { get; set; }   // smooth ramp to target
    double Acceleration { get; set; }   // speed units

    Result ApplyPreset(SpeedPreset preset);
}
