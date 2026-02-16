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
internal sealed record ExportOptions(
    bool IncludeFileChanges = true,
    bool IncludeBranchEvents = true,
    bool IncludeMergeDetection = true,
    bool EnrichWithPullRequests = true,
    int MaxCommits = 0,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    string? TimelineName = null
);