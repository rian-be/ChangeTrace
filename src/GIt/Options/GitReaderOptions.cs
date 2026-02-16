namespace ChangeTrace.GIt.Options;

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
internal sealed record GitReaderOptions(
    bool IncludeFileChanges = true,
    int MaxCommits = 0,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null
);