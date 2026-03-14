using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Specifications.Filters;
using ChangeTrace.Core.Specifications.Filters.Commit;

namespace ChangeTrace.Core.Specifications.Queries.Commits;

/// <summary>
/// Specialized queries for working with merge commits and their file events.
/// </summary>
/// <remarks>
/// Provides reusable specifications to:
/// - Select merge commits
/// - Select merge commits by branch
/// - Select file events associated with a specific merge commit
/// </remarks>
internal static class MergeEventQueries
{
    /// <summary>
    /// Returns a specification matching all merge commits.
    /// </summary>
    public static Specification<TraceEvent> AllMerges()
        => new MergeCommitsOnlySpec();

    /// <summary>
    /// Returns a specification matching merge commits on a specific branch.
    /// </summary>
    /// <param name="branchName">The branch to filter merges for.</param>
    public static Specification<TraceEvent> OnBranch(BranchName branchName)
        => new MergeCommitsOnlySpec().And(new ByBranchSpec(branchName));

    /// <summary>
    /// Returns a specification matching file events associated with a specific merge commit SHA.
    /// </summary>
    /// <param name="sha">The SHA of the merge commit.</param>
    public static Specification<TraceEvent> FilesOfMerge(CommitSha sha)
        => new ByCommitSpec(sha).And(new NonMergeCommitSpec());

    /// <summary>
    /// Returns a specification matching merge commits with at least one file changed.
    /// Combines merge detection with file existence.
    /// </summary>
    public static Specification<TraceEvent> MergesWithFiles()
        => new MergeCommitsOnlySpec().And(new HasFilePathSpec());
}