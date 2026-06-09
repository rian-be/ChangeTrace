using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Info;
using ChangeTrace.Core.Models;

namespace ChangeTrace.GIt.Services.Checkpoints.Models;

/// <summary>
/// Serialized trace event snapshot used by export checkpoint patch logs.
/// </summary>
internal sealed record ExportCheckpointTraceEventSnapshot(
    long TimestampValue,
    string Actor,
    string Target,
    string? Metadata,
    string? CommitShaValue,
    string? BranchNameValue,
    int? PullRequestNumberValue,
    string? FilePathValue,
    byte? CommitType,
    byte? BranchType,
    byte? PullRequestType)
{
    public static ExportCheckpointTraceEventSnapshot From(TraceEvent traceEvent)
    {
        return new ExportCheckpointTraceEventSnapshot(
            traceEvent.Core.Timestamp.UnixSeconds,
            traceEvent.Core.Actor.Value,
            traceEvent.Target,
            traceEvent.Metadata?.Metadata,
            traceEvent.Commit?.Sha.Value,
            traceEvent.Branch?.Name.Value,
            traceEvent.PullRequest?.Number.Value,
            traceEvent.Metadata?.FilePath?.Value,
            EncodeCommitType(traceEvent.Commit),
            EncodeBranchType(traceEvent.Branch),
            EncodePullRequestType(traceEvent.PullRequest));
    }

    public TraceEvent ToTraceEvent()
    {
        var timestamp = Timestamp.FromTrustedUnixSeconds(TimestampValue);
        var actor = ActorName.FromTrustedSerialized(Actor);
        var metadata = CreateMetadata();

        if (CommitShaValue is not null)
            return BranchNameValue is not null
                ? CreateMergeOrBranchCommitEvent(timestamp, actor, metadata)
                : CreateCommitEvent(timestamp, actor, metadata);

        if (PullRequestNumberValue is not null)
            return CreatePullRequestEvent(timestamp, actor, metadata);

        return CreateFallbackEvent(timestamp, actor, metadata);
    }

    private TraceEvent AttachPullRequestIfPresent(TraceEvent evt) => PullRequestNumberValue is null ? evt
        : evt.WithPullRequest(
            PullRequestNumber.Create(PullRequestNumberValue.Value).Value,
            DecodePullRequestType(PullRequestType));

    private static BranchEventType DecodeBranchType(byte? value) =>
        value switch
        {
            0 => BranchEventType.BranchCreated,
            1 => BranchEventType.BranchDeleted,
            _ => BranchEventType.Merge,
        };

    private static FileChangeKind DecodeCommitType(byte? value) =>
        value switch
        {
            1 => FileChangeKind.Added,
            2 => FileChangeKind.Modified,
            3 => FileChangeKind.Deleted,
            4 => FileChangeKind.Renamed,
            _ => FileChangeKind.Commit,
        };

    private static byte? EncodeCommitType(CommitInfo? commit)
    {
        if (commit is null)
            return null;

        return commit.Value.Type switch
        {
            FileChangeKind.Commit => 0,
            FileChangeKind.Added => 1,
            FileChangeKind.Modified => 2,
            FileChangeKind.Deleted => 3,
            _ => 4,
        };
    }

    private static byte? EncodeBranchType(BranchInfo? branch)
    {
        if (branch is null)
            return null;

        return branch.Value.Type switch
        {
            BranchEventType.BranchCreated => 0,
            BranchEventType.BranchDeleted => 1,
            _ => 2,
        };
    }

    private static byte? EncodePullRequestType(PullRequestInfo? pullRequest)
    {
        if (pullRequest is null)
            return null;

        return pullRequest.Value.Type switch
        {
            PullRequestEventType.PullRequestCreated => 0,
            PullRequestEventType.PullRequestMerged => 1,
            _ => 2,
        };
    }

    private static PullRequestEventType DecodePullRequestType(byte? value) =>
        value switch
        {
            1 => PullRequestEventType.PullRequestMerged,
            2 => PullRequestEventType.PullRequestClosed,
            _ => PullRequestEventType.PullRequestCreated,
        };

    private MetadataInfo? CreateMetadata()
        => Metadata is null
            ? null
            : new MetadataInfo(
                Metadata,
                FilePathValue is null ? null : FilePath.FromTrustedSerialized(FilePathValue));

    private TraceEvent CreateMergeOrBranchCommitEvent(
        Timestamp timestamp,
        ActorName actor,
        MetadataInfo? metadata)
    {
        var sha = CommitSha.FromTrustedSerialized(CommitShaValue!);
        var branch = BranchName.FromTrustedSerialized(BranchNameValue!);

        return AttachPullRequestIfPresent(new TraceEvent(
            new TraceEventCore(timestamp, actor, branch.Value),
            Commit: new CommitInfo(sha, DecodeCommitType(CommitType)),
            Branch: new BranchInfo(branch, DecodeBranchType(BranchType)),
            Metadata: metadata));
    }

    private TraceEvent CreateCommitEvent(
        Timestamp timestamp,
        ActorName actor,
        MetadataInfo? metadata)
    {
        var sha = CommitSha.FromTrustedSerialized(CommitShaValue!);

        return AttachPullRequestIfPresent(new TraceEvent(
            new TraceEventCore(timestamp, actor, sha.Value),
            Commit: new CommitInfo(sha, DecodeCommitType(CommitType)),
            Metadata: metadata));
    }

    private TraceEvent CreatePullRequestEvent(
        Timestamp timestamp,
        ActorName actor,
        MetadataInfo? metadata)
    {
        BranchInfo? branchInfo = BranchNameValue is null
            ? null
            : new BranchInfo(
                BranchName.FromTrustedSerialized(BranchNameValue),
                DecodeBranchType(BranchType));

        return new TraceEvent(
            new TraceEventCore(timestamp, actor, BranchNameValue ?? string.Empty),
            Branch: branchInfo,
            PullRequest: new PullRequestInfo(
                PullRequestNumber.Create(PullRequestNumberValue!.Value).Value,
                DecodePullRequestType(PullRequestType)),
            Metadata: metadata);
    }

    private TraceEvent CreateFallbackEvent(Timestamp timestamp, ActorName actor, MetadataInfo? metadata) =>
        new(new TraceEventCore(timestamp, actor, Target), Metadata: metadata);
}
