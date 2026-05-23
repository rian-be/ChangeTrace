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
/// This DTO captures all relevant properties of a timeline event, including
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
        Timestamp = evt.Core.Timestamp.UnixSeconds,
        Actor = evt.Core.Actor.Value,
        Target = evt.Target,
        Metadata = evt.Metadata?.Metadata,
        CommitSha = evt.Commit?.Sha.Value,
        BranchName = evt.Branch?.Name.Value,
        PullRequestNumber = evt.PullRequest?.Number.Value,
        FilePath = evt.Metadata?.FilePath,
        CommitType = evt.Commit?.Type.ToString(),
        BranchType = evt.Branch?.Type.ToString(),
        PrType = evt.PullRequest?.Type.ToString()
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
        var prNum = PullRequestNumber.HasValue
            ? TryCreate(() => Model.PullRequestNumber.Create(PullRequestNumber.Value))
            : (PullRequestNumber?)null;
        
        var evt = (sha, branch, FilePath, CommitType, BranchType) switch
        {
            (not null, not null, _, _, "Merge")
                => TraceEventFactory.Merge(timestamp, actor, sha, branch, Metadata),
            (not null, _, not null, not null, _)
                => CreateFileChange(timestamp, actor, sha),
            (_, not null, _, _, not null)
                => CreateBranch(timestamp, actor, branch, sha),
            (not null, _, _, _, _) => TraceEventFactory.Commit(timestamp, actor, sha, Metadata),
            _ => null
        };
        
        if (evt != null && prNum != null && PrType != null && TryParseEnum<PullRequestEventType>(PrType) is var prType && prType != null)
        {
            evt.Value.WithPullRequest(prNum.Value, prType.Value);
        }
        
        return evt;
    }

    private TraceEvent? CreateFileChange(
        Timestamp timestamp,
        ActorName actor,
        CommitSha sha)
    {
        var path = TryCreate(() => Model.FilePath.Create(FilePath));
        var changeType = TryParseEnum<FileChangeKind>(CommitType);

        return path != null && changeType != null
            ? TraceEventFactory.FileChange(timestamp, actor, path, changeType.Value, sha, Metadata)
            : null;
    }
    
    private TraceEvent? CreateBranch(
        Timestamp timestamp,
        ActorName actor,
        BranchName branch,
        CommitSha? sha)
    {
        var branchType = TryParseEnum<BranchEventType>(BranchType);

        return branchType != null
            ? TraceEventFactory.Branch(timestamp, actor, branch, branchType.Value, sha, Metadata)
            : null;
    }
    
    private static TEnum? TryParseEnum<TEnum>(string? value) where TEnum : struct =>
        value != null && Enum.TryParse<TEnum>(value, out var r) ? r : null;
    
    private static T? TryCreate<T>(Func<Result<T>> factory)
    {
        var result = factory();
        return result.IsSuccess ? result.Value : default;
    }
}
