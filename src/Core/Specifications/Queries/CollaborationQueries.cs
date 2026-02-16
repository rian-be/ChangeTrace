using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Specifications.Filters.Collaboration;

namespace ChangeTrace.Core.Specifications.Queries;

/// <summary>
/// Collaboration queries providing reusable specifications for <see cref="TraceEvent"/> instances.
/// These queries combine atomic collaboration filters such as reviews, merges, cross-team work, and pair programming.
/// </summary>
/// <remarks>
/// Use this class to compose specifications for filtering events related to collaboration:
/// - Pull requests reviewed by a specific actor
/// - Pull requests merged by a specific actor
/// - Cross-team collaboration (multiple authors in a PR)
/// - Pair programming or multiple authors modifying the same file in a short time window
/// </remarks>
internal static class CollaborationSpecs
{
    /// <summary>
    /// Creates specification for pull requests reviewed by a specific actor.
    /// </summary>
    /// <param name="reviewer">The actor who reviewed the pull request.</param>
    /// <returns>A specification matching pull request reviews by the given <paramref name="reviewer"/>.</returns>
    public static Specification<TraceEvent> ReviewedBy(ActorName reviewer)
        => new PullRequestReviewBySpec(reviewer);

    /// <summary>
    /// Creates specification for pull requests merged by a specific actor.
    /// </summary>
    /// <param name="merger">The actor who merged the pull request.</param>
    /// <returns>A specification matching pull request merges by the given <paramref name="merger"/>.</returns>
    public static Specification<TraceEvent> MergedBy(ActorName merger)
        => new PullRequestMergedBySpec(merger);

    /// <summary>
    /// Creates specification to detect cross-team collaboration (PRs with multiple authors).
    /// </summary>
    /// <returns>A specification matching pull requests authored by multiple actors.</returns>
    public static Specification<TraceEvent> CrossTeamWork()
        => new DifferentAuthorsInSamePrSpec();

    /// <summary>
    /// Creates specification to detect pair programming or multiple authors modifying the same file in a short time window.
    /// </summary>
    /// <returns>A specification matching co-edits in the same file within a short interval.</returns>
    public static Specification<TraceEvent> PairProgramming()
        => new SameFileDifferentAuthorsShortWindowSpec();

    /// <summary>
    /// Combines review and merge filters into a single specification.
    /// </summary>
    /// <param name="reviewer">Actor who reviewed the PR.</param>
    /// <param name="merger">Actor who merged the PR.</param>
    /// <returns>
    /// A composite specification matching PRs reviewed by <paramref name="reviewer"/>
    /// and merged by <paramref name="merger"/>.
    /// </returns>
    public static Specification<TraceEvent> ReviewedAndMergedBy(ActorName reviewer, ActorName merger)
        => ReviewedBy(reviewer).And(MergedBy(merger));
}