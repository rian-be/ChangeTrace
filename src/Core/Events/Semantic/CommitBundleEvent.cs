namespace ChangeTrace.Core.Events.Semantic;

/// <summary>
/// Represents commit-based semantic event containing metadata and affected files.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks the commit via <see cref="CommitSha"/>.</item>
/// <item>Records the actor (author or committer) via <see cref="Actor"/>.</item>
/// <item>Contains the virtual timestamp of the event via <see cref="Timestamp"/>.</item>
/// <item>Holds the set of changed files in <see cref="Files"/>.</item>
/// <item>Can be implicitly converted to a <see cref="SemanticEvent"/> using <see cref="Timestamp"/>.</item>
/// </list>
/// </remarks>
internal readonly struct CommitBundleEvent(string commitSha, string actor, double timestamp, ReadOnlyMemory<string> files)
{
    /// <summary>
    /// Gets SHA of the commit this event represents.
    /// </summary>
    public readonly string CommitSha = commitSha;

    /// <summary>
    /// Gets an actor responsible for commit (author or committer).
    /// </summary>
    public readonly string Actor = actor;

    /// <summary>
    /// Gets a virtual timestamp of an event.
    /// </summary>
    public readonly double Timestamp = timestamp;

    /// <summary>
    /// Gets files affected by this commit.
    /// </summary>
    public readonly ReadOnlyMemory<string> Files = files;

    /// <summary>
    /// Implicitly converts <see cref="CommitBundleEvent"/> to <see cref="SemanticEvent"/>.
    /// </summary>
    /// <param name="e">The commit bundle event.</param>
    public static implicit operator SemanticEvent(CommitBundleEvent e)
        => new SemanticEvent(e.Timestamp);
}