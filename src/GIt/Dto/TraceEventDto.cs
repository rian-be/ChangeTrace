using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using MessagePack;
using Model = ChangeTrace.Core.Models;

namespace ChangeTrace.GIt.Dto;

/// <summary>
/// Data Transfer Object representing a <see cref="TraceEvent"/>.
/// Used for serialization, persistence, and inter-process transfer.
/// </summary>
/// <remarks>
/// This DTO captures all relevant properties of a timeline event, including:
/// - Commit, branch, and pull request metadata  
/// - File changes  
/// - Merge events  
/// Provides conversion to/from the domain <see cref="TraceEvent"/> via <see cref="FromDomain"/> and <see cref="ToDomain"/>.
/// Immutable record with optional fields to allow partial data.
/// </remarks>
[MessagePackObject(AllowPrivate = true)]
internal sealed record TraceEventDto
{
    [Key(0)] internal long Timestamp { get; init; }
    [Key(1)] internal string Actor { get; init; } = string.Empty;
    [Key(2)] internal string Target { get; init; } = string.Empty;
    [Key(3)] internal string? Metadata { get; init; }

    [Key(4)] internal string? CommitSha { get; init; }
    [Key(5)] internal string? BranchName { get; init; }
    [Key(6)] internal int? PullRequestNumber { get; init; }
    [Key(7)] internal string? FilePath { get; init; }
    [Key(8)] internal string? CommitType { get; init; }
    [Key(9)] internal string? BranchType { get; init; }
    [Key(10)] internal string? PrType { get; init; }

    /// <summary>
    /// Converts a domain TraceEvent to a DTO.
    /// </summary>
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

    /// <summary>
    /// Converts this DTO back into a domain TraceEvent.
    /// Returns null if creation fails.
    /// </summary>
    internal TraceEvent? ToDomain()
    {
        var timestamp = TryCreate(() => Model.Timestamp.Create(Timestamp));
        var actor = TryCreate(() => ActorName.Create(Actor));
        if (actor == null) return null;

        var sha = CommitSha != null ? TryCreate(() => Model.CommitSha.Create(CommitSha)) : null;
        var branch = BranchName != null ? TryCreate(() => Model.BranchName.Create(BranchName)) : null;
        var prNum = PullRequestNumber.HasValue ? TryCreate(() => Model.PullRequestNumber.Create(PullRequestNumber.Value)) : (PullRequestNumber?)null;

        // Candidate factories for different event types
        var candidates = new Func<TraceEvent?>[]
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

        // Enrich with PR data if present
        if (evt != null && prNum != null && PrType != null && TryParseEnum<PullRequestEventType>(PrType) is var prType && prType != null)
        {
            evt.EnrichWithPullRequest(prNum.Value, prType.Value);
        }

        return evt;
    }

    // Helper: Try to parse enum from string
    private static TEnum? TryParseEnum<TEnum>(string? value) where TEnum : struct =>
        value != null && Enum.TryParse<TEnum>(value, out var r) ? r : null;

    // Helper: Try to create a Result<T> and return value or null
    private static T? TryCreate<T>(Func<Result<T>> factory) => factory().IsSuccess ? factory().Value : default;
}
