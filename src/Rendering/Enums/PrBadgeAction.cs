namespace ChangeTrace.Rendering.Enums;

/// <summary>
/// Actions for pull request badges displayed on branch nodes in renderer.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="Open"/> – PR has been opened; show the badge.</item>
/// <item><see cref="Close"/> – PR has been closed without merging; remove the badge.</item>
/// <item><see cref="Merge"/> – PR has been merged; trigger merge badge effect.</item>
/// </list>
/// </remarks>
internal enum PrBadgeAction
{
    Open,
    Close,
    Merge
}