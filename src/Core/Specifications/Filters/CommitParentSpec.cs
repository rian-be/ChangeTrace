using ChangeTrace.Core.Events;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Specification that checks if <see cref="TraceEvent"/> represents parent (same commit) of given commit SHA.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Matches events that have a non-null <see cref="TraceEvent.Commit"/>.</item>
/// <item>Matches events whose <see cref="TraceEvent.Target"/> equals the specified commit SHA.</item>
/// <item>Matches events whose <see cref="TraceEvent.Core.Target"/> also equals the specified commit SHA (ensuring it's the parent commit itself).</item>
/// </list>
/// </remarks>
internal sealed class CommitParentSpec(string commitSha) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the specified <see cref="TraceEvent"/> satisfies the specification.
    /// </summary>
    /// <param name="item">The trace event to test.</param>
    /// <returns><c>true</c> if the event represents the parent commit; otherwise <c>false</c>.</returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.Commit.HasValue
           && item.Target == commitSha
           && item.Core.Target == commitSha;
}