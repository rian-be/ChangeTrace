namespace ChangeTrace.Rendering.Commands;

using ChangeTrace.Rendering.Enums;

/// <summary>
/// Command to show or hide a pull request badge on a branch node.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="Timestamps"/> – Simulation time when the badge action occurs.</item>
/// <item><see cref="BranchName"/> – Name of the branch associated with the PR.</item>
/// <item><see cref="PrNumber"/> – Pull request number displayed on the badge.</item>
/// <item><see cref="Action"/> – Indicates whether the badge should appear or disappear.</item>
/// </list>
/// </remarks>
internal sealed record PullRequestBadgeCommand(
    double Timestamps,
    string BranchName,
    int PrNumber,
    PrBadgeAction Action
) : RenderCommand(Timestamps);