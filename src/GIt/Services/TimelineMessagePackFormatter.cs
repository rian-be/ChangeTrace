using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using MessagePack;
using MessagePack.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// MessagePack formatter for timelines.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IMessagePackFormatter<Timeline>))]
internal sealed class TimelineMessagePackFormatter : IMessagePackFormatter<Timeline>
{
    /// <summary>
    /// Serializes a timeline to MessagePack.
    /// </summary>
    public void Serialize(ref MessagePackWriter writer, Timeline value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(4);
        writer.WriteNil();

        WriteRepositoryId(ref writer, value);
        WriteEvents(ref writer, value);
        writer.Write(IsTimelineNormalized(value));
    }

    /// <summary>
    /// Deserializes a timeline from MessagePack.
    /// </summary>
    public Timeline Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
            throw new InvalidOperationException("Deserialized timeline payload is null.");

        var count = reader.ReadArrayHeader();
        RepositoryId? repositoryId = null;
        Timeline? timeline = null;

        for (var index = 0; index < count; index++)
        {
            switch (index)
            {
                case 0:
                    reader.Skip();
                    break;
                case 1:
                    repositoryId = ReadRepositoryId(ref reader);
                    break;
                case 2:
                    timeline = ReadTimelineEvents(ref reader, repositoryId);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return timeline ?? new Timeline(repositoryId);
    }

    /// <summary>
    /// Writes the repository identifier payload.
    /// </summary>
    internal static void WriteRepositoryId(ref MessagePackWriter writer, RepositoryId? repositoryId)
    {
        if (repositoryId is not { } value)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(2);
        writer.Write(value.Owner);
        writer.Write(value.Name);
    }

    private static void WriteRepositoryId(ref MessagePackWriter writer, Timeline timeline)
    {
        WriteRepositoryId(ref writer, timeline.RepositoryId);
    }

    /// <summary>
    /// Writes timeline events.
    /// </summary>
    private static void WriteEvents(ref MessagePackWriter writer, Timeline timeline)
    {
        var events = timeline.EventsSpan;
        writer.WriteArrayHeader(events.Length);

        for (var index = 0; index < events.Length; index++)
            WriteEvent(ref writer, events[index]);
    }

    /// <summary>
    /// Writes a single timeline event.
    /// </summary>
    internal static void WriteEvent(ref MessagePackWriter writer, TraceEvent traceEvent)
    {
        writer.WriteArrayHeader(11);
        writer.Write(traceEvent.Core.Timestamp.UnixSeconds);
        writer.Write(traceEvent.Core.Actor.Value);
        writer.Write(traceEvent.Target);
        WriteNullableString(ref writer, traceEvent.Metadata?.Metadata);
        WriteNullableString(ref writer, traceEvent.Commit?.Sha.Value);
        WriteNullableString(ref writer, traceEvent.Branch?.Name.Value);
        WriteNullableInt32(ref writer, traceEvent.PullRequest?.Number.Value);
        WriteNullableString(ref writer, traceEvent.Metadata?.FilePath?.Value);
        WriteNullableByte(ref writer, EncodeCommitType(traceEvent.Commit?.Type));
        WriteNullableByte(ref writer, EncodeBranchType(traceEvent.Branch?.Type));
        WriteNullableByte(ref writer, EncodePullRequestType(traceEvent.PullRequest?.Type));
    }

    /// <summary>
    /// Reads the repository identifier payload.
    /// </summary>
    private static RepositoryId? ReadRepositoryId(ref MessagePackReader reader)
    {
        if (reader.TryReadNil())
            return null;

        var length = reader.ReadArrayHeader();
        string? owner = null;
        string? name = null;

        if (length > 0)
            owner = reader.ReadString();
        if (length > 1)
            name = reader.ReadString();
        for (var index = 2; index < length; index++)
            reader.Skip();

        return owner != null && name != null
            ? RepositoryId.FromTrustedSerialized(owner, name)
            : null;
    }

    /// <summary>
    /// Reads timeline events.
    /// </summary>
    private static Timeline ReadTimelineEvents(
        ref MessagePackReader reader,
        RepositoryId? repositoryId)
    {
        var eventCount = reader.ReadArrayHeader();
        var timeline = new Timeline(repositoryId, eventCount);

        for (var index = 0; index < eventCount; index++)
        {
            if (ReadEvent(ref reader) is { } traceEvent)
                timeline.AddEvent(traceEvent);
        }

        return timeline;
    }

    /// <summary>
    /// Reads a single timeline event.
    /// </summary>
    private static TraceEvent? ReadEvent(ref MessagePackReader reader)
    {
        if (reader.TryReadNil())
            return null;

        var length = reader.ReadArrayHeader();
        long timestampValue = default;
        string? actorValue = null;
        string? metadata = null;
        string? commitSha = null;
        string? branchName = null;
        int? pullRequestNumber = null;
        string? filePath = null;
        byte? commitType = null;
        byte? branchType = null;
        byte? pullRequestType = null;

        for (var index = 0; index < length; index++)
        {
            switch (index)
            {
                case 0:
                    timestampValue = reader.ReadInt64();
                    break;
                case 1:
                    actorValue = reader.ReadString();
                    break;
                case 2:
                    reader.Skip();
                    break;
                case 3:
                    metadata = reader.TryReadNil() ? null : reader.ReadString();
                    break;
                case 4:
                    commitSha = reader.TryReadNil() ? null : reader.ReadString();
                    break;
                case 5:
                    branchName = reader.TryReadNil() ? null : reader.ReadString();
                    break;
                case 6:
                    pullRequestNumber = reader.TryReadNil() ? null : reader.ReadInt32();
                    break;
                case 7:
                    filePath = reader.TryReadNil() ? null : reader.ReadString();
                    break;
                case 8:
                    commitType = ReadNullableByte(ref reader, TryDecodeCommitTypeFromString);
                    break;
                case 9:
                    branchType = ReadNullableByte(ref reader, TryDecodeBranchTypeFromString);
                    break;
                case 10:
                    pullRequestType = ReadNullableByte(ref reader, TryDecodePullRequestTypeFromString);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        var timestamp = Timestamp.FromTrustedUnixSeconds(timestampValue);
        var actor = actorValue != null ? ActorName.FromTrustedSerialized(actorValue) : null;
        if (actor == null)
            return null;

        var sha = commitSha != null ? CommitSha.FromTrustedSerialized(commitSha) : null;
        var branch = branchName != null ? BranchName.FromTrustedSerialized(branchName) : null;
        var prNumber = pullRequestNumber.HasValue
            ? PullRequestNumber.Create(pullRequestNumber.Value).ValueOrNull
            : (PullRequestNumber?)null;

        TraceEvent? traceEvent;

        if (sha != null && branch != null && DecodeBranchType(branchType) == BranchEventType.Merge)
        {
            traceEvent = TraceEventFactory.Merge(timestamp, actor, sha, branch, metadata);
        }
        else if (sha != null && filePath != null && commitType.HasValue)
        {
            traceEvent = CreateFileChangeEvent(timestamp, actor, sha, filePath, commitType.Value, metadata);
        }
        else if (branch != null && branchType.HasValue)
        {
            traceEvent = CreateBranchEvent(timestamp, actor, branch, sha, branchType.Value, metadata);
        }
        else if (sha != null)
        {
            traceEvent = TraceEventFactory.Commit(timestamp, actor, sha, metadata);
        }
        else
        {
            traceEvent = null;
        }

        if (traceEvent is { } eventValue &&
            prNumber != null &&
            DecodePullRequestType(pullRequestType) is { } prType)
        {
            traceEvent = eventValue.WithPullRequest(prNumber.Value, prType);
        }

        return traceEvent;
    }

    /// <summary>
    /// Creates a file change event.
    /// </summary>
    private static TraceEvent? CreateFileChangeEvent(
        Timestamp timestamp,
        ActorName actor,
        CommitSha sha,
        string filePath,
        byte commitType,
        string? metadata)
    {
        var path = FilePath.FromTrustedSerialized(filePath);
        var type = DecodeCommitType(commitType);

        return type != null
            ? TraceEventFactory.FileChange(timestamp, actor, path, type.Value, sha, metadata)
            : null;
    }

    /// <summary>
    /// Creates a branch event.
    /// </summary>
    private static TraceEvent? CreateBranchEvent(
        Timestamp timestamp,
        ActorName actor,
        BranchName branch,
        CommitSha? sha,
        byte branchType,
        string? metadata)
    {
        var type = DecodeBranchType(branchType);

        return type != null
            ? TraceEventFactory.Branch(timestamp, actor, branch, type.Value, sha, metadata)
            : null;
    }

    /// <summary>
    /// Encodes a file change type.
    /// </summary>
    private static byte? EncodeCommitType(FileChangeKind? type) => type switch
    {
        FileChangeKind.Commit => 0,
        FileChangeKind.Added => 1,
        FileChangeKind.Modified => 2,
        FileChangeKind.Deleted => 3,
        FileChangeKind.Renamed => 4,
        _ => null
    };

    /// <summary>
    /// Encodes a branch event type.
    /// </summary>
    private static byte? EncodeBranchType(BranchEventType? type) => type switch
    {
        BranchEventType.BranchCreated => 0,
        BranchEventType.BranchDeleted => 1,
        BranchEventType.Merge => 2,
        _ => null
    };

    /// <summary>
    /// Encodes a pull request event type.
    /// </summary>
    private static byte? EncodePullRequestType(PullRequestEventType? type) => type switch
    {
        PullRequestEventType.PullRequestCreated => 0,
        PullRequestEventType.PullRequestMerged => 1,
        PullRequestEventType.PullRequestClosed => 2,
        _ => null
    };

    /// <summary>
    /// Decodes a file change type.
    /// </summary>
    private static FileChangeKind? DecodeCommitType(byte? value) => value switch
    {
        0 => FileChangeKind.Commit,
        1 => FileChangeKind.Added,
        2 => FileChangeKind.Modified,
        3 => FileChangeKind.Deleted,
        4 => FileChangeKind.Renamed,
        _ => null
    };

    /// <summary>
    /// Decodes a branch event type.
    /// </summary>
    private static BranchEventType? DecodeBranchType(byte? value) => value switch
    {
        0 => BranchEventType.BranchCreated,
        1 => BranchEventType.BranchDeleted,
        2 => BranchEventType.Merge,
        _ => null
    };

    /// <summary>
    /// Decodes a pull request event type.
    /// </summary>
    private static PullRequestEventType? DecodePullRequestType(byte? value) => value switch
    {
        0 => PullRequestEventType.PullRequestCreated,
        1 => PullRequestEventType.PullRequestMerged,
        2 => PullRequestEventType.PullRequestClosed,
        _ => null
    };

    /// <summary>
    /// Decodes a legacy file change type string.
    /// </summary>
    private static byte? TryDecodeCommitTypeFromString(string? value) => value switch
    {
        nameof(FileChangeKind.Commit) => 0,
        nameof(FileChangeKind.Added) => 1,
        nameof(FileChangeKind.Modified) => 2,
        nameof(FileChangeKind.Deleted) => 3,
        nameof(FileChangeKind.Renamed) => 4,
        _ => null
    };

    /// <summary>
    /// Decodes a legacy branch event type string.
    /// </summary>
    private static byte? TryDecodeBranchTypeFromString(string? value) => value switch
    {
        nameof(BranchEventType.BranchCreated) => 0,
        nameof(BranchEventType.BranchDeleted) => 1,
        nameof(BranchEventType.Merge) => 2,
        _ => null
    };

    /// <summary>
    /// Decodes a legacy pull request event type string.
    /// </summary>
    private static byte? TryDecodePullRequestTypeFromString(string? value) => value switch
    {
        nameof(PullRequestEventType.PullRequestCreated) => 0,
        nameof(PullRequestEventType.PullRequestMerged) => 1,
        nameof(PullRequestEventType.PullRequestClosed) => 2,
        _ => null
    };

    /// <summary>
    /// Writes an optional string value.
    /// </summary>
    private static void WriteNullableString(ref MessagePackWriter writer, string? value)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        writer.Write(value);
    }

    /// <summary>
    /// Writes an optional 32-bit integer value.
    /// </summary>
    private static void WriteNullableInt32(ref MessagePackWriter writer, int? value)
    {
        if (!value.HasValue)
        {
            writer.WriteNil();
            return;
        }

        writer.Write(value.Value);
    }

    /// <summary>
    /// Writes an optional byte value.
    /// </summary>
    private static void WriteNullableByte(ref MessagePackWriter writer, byte? value)
    {
        if (!value.HasValue)
        {
            writer.WriteNil();
            return;
        }

        writer.Write(value.Value);
    }

    /// <summary>
    /// Reads an optional byte value.
    /// </summary>
    private static byte? ReadNullableByte(
        ref MessagePackReader reader,
        Func<string?, byte?> legacyStringDecoder)
    {
        if (reader.TryReadNil())
            return null;

        return reader.NextMessagePackType switch
        {
            MessagePackType.Integer => reader.ReadByte(),
            MessagePackType.String => legacyStringDecoder(reader.ReadString()),
            _ => SkipUnsupportedByteLike(ref reader)
        };
    }

    /// <summary>
    /// Skips an unsupported byte-like value.
    /// </summary>
    private static byte? SkipUnsupportedByteLike(ref MessagePackReader reader)
    {
        reader.Skip();
        return null;
    }

    /// <summary>
    /// Checks whether the timeline is ordered by timestamp.
    /// </summary>
    private static bool IsTimelineNormalized(Timeline timeline)
    {
        var events = timeline.EventsSpan;

        if (events.Length <= 1)
            return true;

        var previousTimestamp = events[0].Core.Timestamp;

        for (var index = 1; index < events.Length; index++)
        {
            var currentTimestamp = events[index].Core.Timestamp;

            if (currentTimestamp < previousTimestamp)
                return false;

            previousTimestamp = currentTimestamp;
        }

        return true;
    }
}
