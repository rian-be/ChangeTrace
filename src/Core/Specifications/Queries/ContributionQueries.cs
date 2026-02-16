using ChangeTrace.Core.Models;
using ChangeTrace.Core.Specifications.Filters;
using ChangeTrace.Core.Specifications.Filters.Contribution;

namespace ChangeTrace.Core.Specifications.Queries;

/// <summary>
/// Contribution queries providing reusable specifications for <see cref="TraceEvent"/> instances.
/// These queries allow filtering events based on contributor activity and patterns.
/// </summary>
/// <remarks>
/// Use this class to compose specifications for filtering events by contributor behavior:
/// - Commits by a specific actor
/// - First-time contributors
/// - Returning contributors after a period of inactivity
/// - High-frequency contributors
/// </remarks>
internal static class ContributionQueries
{
    /// <summary>
    /// Creates specification for commits made by a specific actor.
    /// </summary>
    /// <param name="actor">The actor whose contributions should be selected.</param>
    /// <returns>A specification matching commits authored by the given <paramref name="actor"/>.</returns>
    public static Specification<TraceEvent> ByActor(ActorName actor)
        => new ByActorSpec(actor);

    /// <summary>
    /// Creates specification for first-time contributors.
    /// </summary>
    /// <returns>A specification matching commits that are the first by an author.</returns>
    public static Specification<TraceEvent> FirstTimeContributors()
        => new FirstCommitByAuthorSpec();

    /// <summary>
    /// Creates specification for contributors returning after a period of inactivity.
    /// </summary>
    /// <param name="days">The number of days of inactivity after which a contributor is considered returning.</param>
    /// <returns>A specification matching contributors who made commits after being inactive for <paramref name="days"/> days.</returns>
    public static Specification<TraceEvent> ReturningContributors(int days)
        => new ReturningAfterInactivitySpec(days);

    /// <summary>
    /// Creates specification for high-frequency contributors.
    /// </summary>
    /// <param name="commits">The minimum number of commits required to be considered high-frequency.</param>
    /// <returns>A specification matching authors with at least <paramref name="commits"/> commits.</returns>
    public static Specification<TraceEvent> HighFrequencyContributors(int commits)
        => new AuthorCommitCountAboveSpec(commits);
}