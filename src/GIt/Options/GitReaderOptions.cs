namespace ChangeTrace.GIt.Options;

/// <summary>
/// Selects the backend used to read commit history from a local Git repository.
/// </summary>
internal enum GitHistoryReaderBackend
{
    LibGit2Sharp,
    GitCli
}

/// <summary>
/// Options controlling how a Git repository is read.
/// <para>
/// These options are used by GitRepositoryReader to filter commits,
/// limit the number of commits, and optionally include file level changes.
/// </para>
/// </summary>
/// <param name="IncludeFileChanges">
/// Whether to include detailed file-level changes for commits. Defaults to <c>true</c>.
/// </param>
/// <param name="MaxCommits">
/// Maximum number of commits to read. 0 means no limit. Defaults to <c>0</c>.
/// </param>
/// <param name="StartDate">
/// Optional start date filter; only commits on or after this date are included.
/// </param>
/// <param name="EndDate">
/// Optional end date filter; only commits on or before this date are included.
/// </param>
/// <param name="Backend">
/// History reader backend. Defaults to <see cref="GitHistoryReaderBackend.LibGit2Sharp"/> for existing behavior.
/// </param>
/// <param name="DetectRenames">
/// Whether Git CLI file-change extraction should detect renames. Defaults to <c>true</c> for fidelity.
/// Disabling it improves throughput but reports renames as separate delete/add changes.
/// </param>
/// <param name="IncludeBranches">
/// Whether to populate branch metadata for each commit. Defaults to <c>true</c>.
/// Disable it when branch and merge events are not needed to avoid extra branch graph traversal.
/// </param>
internal sealed record GitReaderOptions(
    bool IncludeFileChanges = true,
    int MaxCommits = 0,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    GitHistoryReaderBackend Backend = GitHistoryReaderBackend.LibGit2Sharp,
    bool DetectRenames = true,
    bool IncludeBranches = true
);
