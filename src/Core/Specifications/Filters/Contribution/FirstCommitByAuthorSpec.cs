using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Contribution;

/// <summary>
/// Matches the first commit made by each author.
/// Use to detect when contributors start contributing to the repository.
/// </summary>
internal sealed class FirstCommitByAuthorSpec : Specification<TraceEvent>
{
    // Tracks authors who have already made a commit
    private readonly HashSet<ActorName> _authorsSeen = [];

    /// <summary>
    /// Determines whether the specified <paramref name="item"/> is the first commit by the author.
    /// </summary>
    /// <param name="item">The Git event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the <paramref name="item"/> is a commit and the author's first commit; otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item) =>
        item.CommitType != null && _authorsSeen.Add(item.Actor);
}