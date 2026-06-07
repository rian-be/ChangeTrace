using ChangeTrace.Core;

namespace ChangeTrace.GIt.Options;

/// <summary>
/// Options controlling repository export behavior.
/// <para>
/// These options configure how  Ttmeline is built from a Git repository.
/// They affect which commits, branches, merge, and pull request enrichments are included.
/// </para>
/// </summary>
/// <param name="IncludeFileChanges">
/// Whether to include detailed file-level changes for commits. Defaults to <c>true</c>.
/// This is one of the main export cost drivers on large repositories.
/// </param>
/// <param name="IncludeBranchEvents">
/// Whether to generate branch-related events in the timeline. Defaults to <c>true</c>.
/// </param>
/// <param name="IncludeMergeDetection">
/// Whether to detect merge commits explicitly. Defaults to <c>true</c>.
/// </param>
/// <param name="EnrichWithPullRequests">
/// Whether to enrich the timeline with pull request events (GitHub/GitLab). Defaults to <c>true</c>.
/// </param>
/// <param name="MaxCommits">
/// Maximum number of commits to include. 0 means no limit. Defaults to <c>0</c>.
/// </param>
/// <param name="StartDate">
/// Optional start date filter; only commits on or after this date are included.
/// </param>
/// <param name="EndDate">
/// Optional end date filter; only commits on or before this date are included.
/// </param>
/// <param name="TimelineName">
/// Optional name for the exported timeline.
/// </param>
internal sealed record ExportOptions
{
    public string? GitHubToken { get; init; }
    public string? GitLabToken { get; init; }
    public string GitLabBaseUrl { get; init; } = "https://gitlab.com";
    public bool IncludeFileChanges { get; init; } = true;
    public bool IncludeBranchEvents { get; init; } = true;
    public bool IncludeMergeDetection { get; init; } = true;
    public bool EnrichWithPullRequests { get; init; } = true;
    /// <summary>
    /// History backend used for commit and file-change extraction.
    /// <see cref="GitHistoryReaderBackend.GitCli"/> is usually the better choice for larger local histories.
    /// </summary>
    public GitHistoryReaderBackend HistoryBackend { get; init; } = GitHistoryReaderBackend.LibGit2Sharp;
    /// <summary>
    /// Controls rename detection for Git CLI file change extraction.
    /// Disabling it improves throughput but reports renames as delete/add pairs.
    /// </summary>
    public bool DetectRenames { get; init; } = true;
    public int MaxCommits { get; init; } = 0;
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
}
