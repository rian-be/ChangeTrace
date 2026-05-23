using ChangeTrace.Player.Enums;
using ChangeTrace.Rendering.Enums;
using JetBrains.Annotations;

namespace ChangeTrace.Rendering.Hud;

/// <summary>
/// HUD snapshot describing playback and timeline state.
/// </summary>
[UsedImplicitly]
internal record PlaybackHud(
    string TimeLabel,
    string? AbsoluteDateLabel,
    int ElapsedDays,
    string SpeedLabel,
    float Progress,
    bool IsRamping,
    PlayerState State,
    CameraFollowMode CameraMode,
    LayoutMode LayoutMode);