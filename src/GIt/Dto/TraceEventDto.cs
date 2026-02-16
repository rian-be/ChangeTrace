using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using Model = ChangeTrace.Core.Models;

namespace ChangeTrace.GIt.Dto;

/// <summary>
/// Data Transfer Object representing a <see cref="TraceEvent"/>.
/// Used for serialization, persistence, and inter process transfer.
/// </summary>
/// <remarks>
/// This DTO captures all relevant properties of a timeline event, including:
/// - Commit, branch, and pull request metadata  
/// - File changes  
/// - Merge events  
/// 
/// Provides conversion to/from the domain <see cref="TraceEvent"/> via <see cref="FromDomain"/> and <see cref="ToDomain"/>.
/// Immutable record with optional fields to allow partial data.
/// </remarks>
internal sealed record TraceEventDto
{
    internal long Timestamp { get; init; }
    internal string Actor { get; init; } = string.Empty;
    internal string Target { get; init; } = string.Empty;
    internal string? Metadata { get; init; }
    internal string? CommitSha { get; init; }
    internal string? BranchName { get; init; }
    internal int? PullRequestNumber { get; init; }
    internal string? FilePath { get; init; }
    internal string? CommitType { get; init; }
    internal string? BranchType { get; init; }
    internal string? PrType { get; init; }

    internal static TraceEventDto FromDomain(TraceEvent evt) => new()
    {
        Timestamp = evt.Timestamp.UnixSeconds,
        Actor = evt.Actor.Value,
        Target = evt.Target,
        Metadata = evt.Metadata,
        CommitSha = evt.CommitSha?.Value,
        BranchName = evt.BranchName?.Value,
        PullRequestNumber = evt.PullRequestNumber?.Value,
        FilePath = evt.FilePath?.Value,
        CommitType = evt.CommitType?.ToString(),
        BranchType = evt.BranchType?.ToString(),
        PrType = evt.PrType?.ToString()
    };

    internal TraceEvent? ToDomain()
    {
        var timestamp = TryCreate(() => Model.Timestamp.Create(Timestamp));
        var actor = TryCreate(() => ActorName.Create(Actor));
        if (actor == null) return null;

        var sha = CommitSha != null ? TryCreate(() => Model.CommitSha.Create(CommitSha)) : null;
        var branch = BranchName != null ? TryCreate(() => Model.BranchName.Create(BranchName)) : null;
        var prNum = PullRequestNumber.HasValue ? TryCreate(() => Model.PullRequestNumber.Create(PullRequestNumber.Value)) : (PullRequestNumber?)null;
        
        var candidates = new[]
        {
            // Merge
            () => (sha != null && BranchType == "Merge") 
                ? TraceEventFactory.Merge(timestamp, actor, sha, branch, Metadata) 
                : null,

            // File change
            () => (sha != null && FilePath != null && CommitType != null) 
                ? (TryCreate(() => Model.FilePath.Create(FilePath)) is var path && path != null &&
                   TryParseEnum<CommitEventType>(CommitType) is var changeType && changeType != null
                    ? TraceEventFactory.FileChange(timestamp, actor, path, changeType.Value, sha, Metadata)
                    : null)
                : null,

            // Regular commit
            () => (sha != null) ? TraceEventFactory.Commit(timestamp, actor, sha, Metadata) : null,

            // Branch
            () => (branch != null && BranchType != null && TryParseEnum<BranchEventType>(BranchType) is var branchType && branchType != null)
                ? TraceEventFactory.Branch(timestamp, actor, branch, branchType.Value, sha, Metadata)
                : null
        };

        var evt = candidates.Select(f => f()).FirstOrDefault(e => e != null);

        if (evt != null && prNum != null && PrType != null && TryParseEnum<PullRequestEventType>(PrType) is var prType && prType != null)
        {
            evt.EnrichWithPullRequest(prNum.Value, prType.Value);
        }

        return evt;
    }

    private static TEnum? TryParseEnum<TEnum>(string? value) where TEnum : struct =>
        value != null && Enum.TryParse<TEnum>(value, out var r) ? r : null;

    private static T? TryCreate<T>(Func<Result<T>> factory) => factory().IsSuccess ? factory().Value : default;
}