namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents commit in the repository with all relevant metadata.
/// Contains the core commit information extracted from Git, including identifiers,
/// authorship, message, parent relationships, file changes, and branch associations.
/// Used as the primary data structure for building and analyzing commit history.
/// </summary>
/// <param name="Sha">The unique SHA-1 hash identifying the commit.</param>
/// <param name="Author">The name of the commit author.</param>
/// <param name="Timestamp">Unix timestamp of when the commit was created.</param>
/// <param name="Message">The commit message (short form, first line only).</param>
/// <param name="ParentShas">List of parent commit SHAs. Empty for initial commit, multiple for merge commits.</param>
/// <param name="FileChanges">List of file changes (added, modified, deleted, renamed) in this commit. Empty if file changes not requested.</param>
/// <param name="Branches">List of branch names that contain this commit. Multiple if commit belongs to several branches.</param>
/// <param name="IsMerge">Indicates whether this is a merge commit (has multiple parents).</param>
internal sealed record CommitData(
    CommitSha Sha,
    ActorName Author,
    Timestamp Timestamp,
    string Message,
    IReadOnlyList<CommitSha> ParentShas,
    IReadOnlyList<FileChange> FileChanges,
    IReadOnlyList<BranchName> Branches,
    bool IsMerge
);