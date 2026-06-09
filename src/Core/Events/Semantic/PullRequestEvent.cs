namespace ChangeTrace.Core.Events.Semantic;

using Enums;
using Models;

/// <summary>
/// Represents a pull request event for playback and rendering.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks the event timestamp via <see cref="Timestamp"/>.</item>
/// <item>Records the actor responsible for the event via <see cref="Actor"/>.</item>
/// <item>Records the branch associated with the pull request via <see cref="Branch"/>.</item>
/// <item>Records the pull request number via <see cref="Number"/>.</item>
/// <item>Indicates the pull request state via <see cref="Type"/>.</item>
/// <item>Records the render target via <see cref="Target"/>.</item>
/// <item>Can be implicitly converted to <see cref="SemanticEvent"/> for generic semantic pipelines.</item>
/// </list>
/// </remarks>
internal readonly struct PullRequestEvent(
    double timestamp,
    string actor,
    string branch,
    PullRequestNumber number,
    PullRequestEventType type,
    string target)
{
    /// <summary>Gets the event timestamp (Unix seconds).</summary>
    public readonly double Timestamp = timestamp;

    /// <summary>Gets the actor responsible for the pull request event.</summary>
    public readonly string Actor = actor;

    /// <summary>Gets the branch associated with the pull request event.</summary>
    public readonly string Branch = branch;

    /// <summary>Gets the pull request number.</summary>
    public readonly PullRequestNumber Number = number;

    /// <summary>Gets the pull request event type.</summary>
    public readonly PullRequestEventType Type = type;

    /// <summary>Gets the render target associated with the event.</summary>
    public readonly string Target = target;

    /// <summary>Implicitly converts this event to a <see cref="SemanticEvent"/>.</summary>
    /// <param name="e">The pull request event.</param>
    public static implicit operator SemanticEvent(PullRequestEvent e)
        => new(e.Timestamp);
}
