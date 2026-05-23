namespace ChangeTrace.Rendering.Hud;

/// <summary>
/// HUD interaction state for hovered scene elements.
/// </summary>
internal record InteractionHud(
    string? HoveredNodeId,
    string? HoveredNodeAuthor,
    string? HoveredNodeCommit,
    HoveredPodHud? HoveredPod = null);