using ChangeTrace.Core;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Interfaces;

namespace ChangeTrace.Player.Factory;

internal interface ITimelinePlayerFactory
{
    ITimelinePlayer Create(
        Timeline timeline,
        PlaybackMode mode = PlaybackMode.Once,
        double initialSpeed = 1.0,
        double acceleration = 1.0,
        double secondsPerDay  = 1.0);
}