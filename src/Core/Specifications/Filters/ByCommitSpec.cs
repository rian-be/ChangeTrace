using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Filters events associated with specific commit.
/// 
/// Matches only events that reference commit and whose SHA
/// matches the provided identifier. Comparison may support
/// shortened SHA prefixes depending on <see cref="CommitSha.Matches(CommitSha)"/> semantics.
/// </summary>
/// <param name="commitSha">Commit identifier to match</param>
internal sealed class ByCommitSpec(CommitSha commitSha) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the event refers to the specified commit.
    /// </summary>
    /// <param name="item">Event to evaluate</param>
    /// <returns>
    /// <c>true</c> when the event commit matches the configured SHA;
    /// otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.CommitSha != null && item.CommitSha.Matches(commitSha);
}