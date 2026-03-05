using ChangeTrace.Core.Events;
using ChangeTrace.Core.Specifications.Filters;

namespace ChangeTrace.Core.Specifications.Queries.Commits;

/// <summary>
/// Queries for filtering commit events based on parent/child relationships.
/// </summary>
internal static class CommitRelationshipQueries
{
    /// <summary>
    /// Returns specification matching the parent commit event for the given commit SHA.
    /// </summary>
    /// <param name="commitSha">The commit SHA to match.</param>
    /// <returns>A specification matching the commit parent event.</returns>
    public static Specification<TraceEvent> Parent(string commitSha)
        => new CommitParentSpec(commitSha);

    /// <summary>
    /// Returns specification matching child events (files) for the given commit SHA.
    /// </summary>
    /// <param name="commitSha">The commit SHA to match.</param>
    /// <returns>A specification matching all child events of the commit.</returns>
    public static Specification<TraceEvent> Children(string commitSha)
        => new CommitChildSpec(commitSha);
}