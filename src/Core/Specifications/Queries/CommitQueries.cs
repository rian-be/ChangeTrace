using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Specifications.Filters;

namespace ChangeTrace.Core.Specifications.Queries;

/// <summary>
/// Commit queries providing reusable specifications for <see cref="TraceEvent"/> instances.
/// These queries combine atomic filters such as author, commit type, and pull request association.
/// </summary>
/// <remarks>
/// Use this class to compose specifications for filtering commits by various criteria:
/// - Author-specific commits
/// - Merge commits
/// - Merge commits associated with pull requests
/// </remarks>
internal static class CommitQueries
{
    /// <summary>
    /// Creates specification for commits authored by a specific actor.
    /// </summary>
    /// <param name="actor">The actor whose commits should be selected.</param>
    /// <returns>
    /// A specification matching commits authored by the given <paramref name="actor"/>.
    /// Combines <see cref="CommitsOnlySpec"/> with <see cref="ByActorSpec"/>.
    /// </returns>
    public static Specification<TraceEvent> ByAuthor(ActorName actor)
        => new CommitsOnlySpec().And(new ByActorSpec(actor));

    /// <summary>
    /// Creates specification for merge commits.
    /// </summary>
    /// <returns>A specification matching commits that are merge commits.</returns>
    public static Specification<TraceEvent> Merges()
        => new MergeCommitsOnlySpec();

    /// <summary>
    /// Creates  specification for merge commits that are associated with pull requests.
    /// </summary>
    /// <returns>
    /// A specification matching merge commits that also correspond to pull request merges.
    /// Combines <see cref="MergeCommitsOnlySpec"/> with <see cref="PullRequestsOnlySpec"/>.
    /// </returns>
    public static Specification<TraceEvent> EnrichedMerges()
        => new MergeCommitsOnlySpec().And(new PullRequestsOnlySpec());
}