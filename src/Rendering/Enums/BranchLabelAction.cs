namespace ChangeTrace.Rendering.Enums;

/// <summary>
/// Defines possible actions for branch label in rendering graph.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="Appear"/> – Display label for a branch at given timestamp.</item>
/// <item><see cref="Disappear"/> – Remove label from graph.</item>
/// </list>
/// </remarks>
internal enum BranchLabelAction
{
    /// <summary>Show branch label.</summary>
    Appear,

    /// <summary>Hide branch label.</summary>
    Disappear
}