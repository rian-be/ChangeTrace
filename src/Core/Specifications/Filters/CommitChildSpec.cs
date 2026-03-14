using ChangeTrace.Core.Events;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Specification that checks if a <see cref="TraceEvent"/> represents a child of a given commit.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Matches events that have a non-null <see cref="TraceEvent.Commit"/>.</item>
/// <item>Matches events whose <see cref="TraceEvent.Target"/> equals the specified commit SHA.</item>
/// <item>Excludes events whose <see cref="TraceEvent.Core.Target"/> equals the specified commit SHA (ensuring it's a child, not the commit itself).</item>
/// </list>
/// </remarks>
internal sealed class CommitChildSpec(string commitSha) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the specified <see cref="TraceEvent"/> satisfies the specification.
    /// </summary>
    /// <param name="item">The trace event to test.</param>
    /// <returns><c>true</c> if the event is a child of the specified commit; otherwise <c>false</c>.</returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.Commit != null
           && item.Target == commitSha
           && item.Core.Target != commitSha;
}