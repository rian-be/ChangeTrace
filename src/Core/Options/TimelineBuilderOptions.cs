using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Options;

/// <summary>
/// Configuration options for building a timeline from commit data.
/// Controls which events are generated and how the timeline is structured.
/// </summary>
/// <param name="IncludeFileChanges">If true, generates individual file change events for each modified file in a commit. Defaults to true.</param>
/// <param name="IncludeBranchEvents">If true, generates branch events (create, delete, checkout) from branch information. Defaults to true.</param>
/// <param name="IncludeMergeDetection">If true, identifies and marks merge commits with special handling. Defaults to true.</param>
/// <param name="RepositoryId"> repository identifier for platform integration.</param>
internal sealed record TimelineBuilderOptions(
    bool IncludeFileChanges = true,
    bool IncludeBranchEvents = true,
    bool IncludeMergeDetection = true,
    RepositoryId? RepositoryId = null
);