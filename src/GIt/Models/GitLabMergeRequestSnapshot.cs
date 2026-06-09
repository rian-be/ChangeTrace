namespace ChangeTrace.GIt.Models;

/// <summary>
/// GitLab merge request snapshot used by the enricher.
/// </summary>
internal sealed record GitLabMergeRequestSnapshot(
    int Iid,
    string? MergeCommitSha,
    string? Sha,
    string? SourceBranch,
    string? TargetBranch,
    string? State,
    DateTimeOffset? MergedAt,
    string? AuthorUsername);
