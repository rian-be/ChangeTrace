using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Events;

/// <summary>
/// Stateless predicates describing how a <see cref="TraceEvent"/> can be interpreted.
/// 
/// IMPORTANT:
/// This class contains interpretation logic, not domain state.
/// It must not mutate events.
/// </summary>
internal static class TraceEventPredicates
{
    /// <summary>
    /// Determines whether the event represents a merge commit.
    /// </summary>
    /// <param name="e">The trace event to evaluate.</param>
    /// <returns>True if the event is a merge commit; otherwise false.</returns>
    public static bool IsMerge(in TraceEvent e)
        => e.BranchType == BranchEventType.Merge;

    /// <summary>
    /// Determines whether the event is associated with a pull request.
    /// </summary>
    /// <param name="e">The trace event to evaluate.</param>
    /// <returns>True if the event has pull request metadata; otherwise false.</returns>
    public static bool HasPullRequest(in TraceEvent e)
        => e.PrType.HasValue;

    /// <summary>
    /// Determines whether the event refers to a specific commit SHA selector.
    /// 
    /// Supports:
    /// - full SHA
    /// - short SHA (>= 7 chars)
    /// - Git-style partial matching
    /// </summary>
    /// <param name="e">The trace event to evaluate.</param>
    /// <param name="selector">The commit selector (full SHA, short SHA, or partial match).</param>
    /// <returns>True if the event matches the commit selector; otherwise false.</returns>
    public static bool MatchesCommit(in TraceEvent e, string selector)
    {
        if (string.IsNullOrWhiteSpace(selector))
            return false;

        if (e.Target == selector)
            return true;

        if (e.CommitSha is null || selector.Length < 7)
            return false;

        var parsed = CommitSha.Create(selector);
        return parsed.IsSuccess && e.CommitSha.Matches(parsed.Value);
    }
}