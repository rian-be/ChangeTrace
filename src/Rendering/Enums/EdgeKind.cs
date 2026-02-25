namespace ChangeTrace.Rendering.Enums;

/// <summary>
/// Represents different types of edges between nodes in rendering graph.
/// </summary>
internal enum EdgeKind
{
    /// <summary>
    /// Edge representing standard commit.
    /// </summary>
    Commit,

    /// <summary>
    /// Edge representing merge between branches.
    /// </summary>
    Merge,

    /// <summary>
    /// Edge representing pull request.
    /// </summary>
    PullRequest
}