namespace ChangeTrace.Core.Enums;

/// <summary>
/// Represents type of pull request event in repository.
/// </summary>
internal enum PullRequestEventType
{
    PullRequestCreated,
    PullRequestMerged,
    PullRequestClosed,
}