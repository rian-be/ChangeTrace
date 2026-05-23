using JetBrains.Annotations;

namespace ChangeTrace.Rendering.Hud;

/// <summary>
/// HUD snapshot describing hovered pod overlay state.
/// </summary>
[UsedImplicitly]
internal sealed record HoveredPodHud(
    string Id,
    string Label,
    Vec2 Center,
    Vec2 LabelPosition,
    float Radius,
    float ActivityScore,
    float ImportanceScore,
    IReadOnlyList<string> FileIds);