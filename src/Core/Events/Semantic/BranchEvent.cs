namespace ChangeTrace.Core.Events.Semantic;

using Enums;
using Models;

/// <summary>
/// Represents an event related to branch, such as creation, deletion, or merge.
/// Useful for tracking branch lifecycle and rendering branch-related activity.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks the virtual time of the event via <see cref="Timestamp"/>.</item>
/// <item>Records the actor responsible for the event via <see cref="Actor"/>.</item>
/// <item>Records the branch name via <see cref="Branch"/>.</item>
/// <item>Indicates the type of branch event via <see cref="Type"/>.</item>
/// <item>Optionally records the associated commit SHA via <see cref="Sha"/>.</item>
/// <item>Can be implicitly converted to <see cref="SemanticEvent"/> to integrate with generic semantic pipelines.</item>
/// </list>
/// </remarks>
internal readonly struct BranchEvent(
    double timestamp,
    string actor,
    string branch,
    BranchEventType type,
    CommitSha? sha = null)
{
    /// <summary>Gets the event timestamp (Unix seconds).</summary>
    public readonly double Timestamp = timestamp;

    /// <summary>Gets the actor responsible for the branch event.</summary>
    public readonly string Actor = actor;

    /// <summary>Gets the branch name affected by this event.</summary>
    public readonly string Branch = branch;

    /// <summary>Gets the type of branch event.</summary>
    public readonly BranchEventType Type = type;

    /// <summary>Gets the optional associated commit SHA.</summary>
    public readonly CommitSha? Sha = sha;

    /// <summary>Implicitly converts this event to a <see cref="SemanticEvent"/>.</summary>
    /// <param name="e">The branch event.</param>
    public static implicit operator SemanticEvent(BranchEvent e)
        => new(e.Timestamp);
}