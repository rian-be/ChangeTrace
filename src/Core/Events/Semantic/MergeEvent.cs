namespace ChangeTrace.Core.Events.Semantic;

/// <summary>
/// Represents merge commit event, capturing the actor, source and target branches, 
/// and the files merged.
/// Useful for visualizing merges and tracing file changes across branches.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks the event timestamp via <see cref="Timestamp"/>.</item>
/// <item>Records the actor performing the merge via <see cref="Actor"/>.</item>
/// <item>Records the source branch of the merge via <see cref="SourceBranch"/>.</item>
/// <item>Records the target branch of the merge via <see cref="TargetBranch"/>.</item>
/// <item>Records the files involved in the merge via <see cref="FilesMerged"/>.</item>
/// <item>Can be implicitly converted to <see cref="SemanticEvent"/> for integration with generic semantic pipelines.</item>
/// </list>
/// </remarks>
internal readonly struct MergeEvent(
    double timestamp,
    string actor,
    string sourceBranch,
    string targetBranch,
    ReadOnlyMemory<string> filesMerged)
{
    /// <summary>Gets the event timestamp (Unix seconds).</summary>
    public readonly double Timestamp = timestamp;

    /// <summary>Gets the actor performing the merge.</summary>
    public readonly string Actor = actor;

    /// <summary>Gets the source branch of the merge.</summary>
    public readonly string SourceBranch = sourceBranch;

    /// <summary>Gets the target branch of the merge.</summary>
    public readonly string TargetBranch = targetBranch;

    /// <summary>Gets the files involved in the merge.</summary>
    public readonly ReadOnlyMemory<string> FilesMerged = filesMerged;

    /// <summary>Implicitly converts this merge event to a <see cref="SemanticEvent"/>.</summary>
    /// <param name="e">The merge event.</param>
    public static implicit operator SemanticEvent(MergeEvent e)
        => new(e.Timestamp);
}