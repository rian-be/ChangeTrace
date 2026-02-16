using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Contribution;

/// <summary>
/// Matches events for authors whose total number of commits
/// exceeds a specified threshold.
/// Useful for filtering prolific contributors.
/// </summary>
internal sealed class AuthorCommitCountAboveSpec(int minCommits) : Specification<TraceEvent>
{
    private readonly Dictionary<ActorName, int> _authorCommitCounts = new();

    /// <summary>
    /// Determines whether the given <paramref name="item"/> satisfies the specification.
    /// </summary>
    /// <param name="item">The event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the event is a commit and the author's total commits
    /// exceed the threshold; otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
    {
        if (item.CommitType == null)
            return false;
        var count = _authorCommitCounts.GetValueOrDefault(item.Actor, 0);
        count++;
        
        _authorCommitCounts[item.Actor] = count;
        return count >= minCommits;
    }
}