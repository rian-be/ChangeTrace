using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Dto.Common;
using MessagePack;
using MessagePack.Resolvers;

namespace ChangeTrace.GIt.Dto.Sidecars;

/// <summary>
/// Merge sidecar payload for timeline exports.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
internal sealed record MergeSidecarDto
{
    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithCompression(MessagePackCompression.Lz4BlockArray)
        .WithResolver(StandardResolverAllowPrivate.Instance);

    [Key(0)] internal RepositoryIdDto? RepositoryId { get; init; }
    [Key(1)] internal required List<MergeAttachmentDto> Attachments { get; init; }

    /// <summary>
    /// Creates a sidecar payload from a timeline.
    /// </summary>
    internal static MergeSidecarDto FromDomain(Timeline timeline)
    {
        var events = timeline.EventsSpan;
        var attachments = new List<MergeAttachmentDto>();

        for (var index = 0; index < events.Length; index++)
        {
            var branch = events[index].Branch;
            if (branch is not { Type: BranchEventType.Merge } || events[index].PullRequest is not null)
                continue;

            attachments.Add(new MergeAttachmentDto(index, branch.Value.Name.Value));
        }

        return new MergeSidecarDto
        {
            RepositoryId = timeline.RepositoryId is { } repositoryId
                ? new RepositoryIdDto(repositoryId.Owner, repositoryId.Name)
                : null,
            Attachments = attachments
        };
    }

    /// <summary>
    /// Serializes the sidecar payload.
    /// </summary>
    internal byte[] ToBytes()
        => MessagePackSerializer.Serialize(this, Options);

    /// <summary>
    /// Deserializes the sidecar payload.
    /// </summary>
    internal static MergeSidecarDto FromBytes(byte[] bytes)
        => MessagePackSerializer.Deserialize<MergeSidecarDto>(bytes, Options);

    /// <summary>
    /// Applies merge attachments to a timeline.
    /// </summary>
    internal int ApplyTo(Timeline timeline)
    {
        if (!MatchesRepository(timeline))
            return 0;

        var applied = 0;
        foreach (var attachment in Attachments)
        {
            if (!timeline.TryUpdateAt(
                    attachment.EventIndex,
                    evt =>
                    {
                        var branchName = BranchName.FromTrustedSerialized(attachment.TargetBranch);
                        return evt.WithBranch(branchName, BranchEventType.Merge);
                    }))
            {
                continue;
            }

            applied++;
        }

        return applied;
    }

    /// <summary>
    /// Gets whether the payload contains attachments.
    /// </summary>
    [IgnoreMember]
    internal bool HasAttachments => Attachments.Count > 0;

    private bool MatchesRepository(Timeline timeline)
    {
        if (RepositoryId is null || timeline.RepositoryId is null)
            return true;

        return string.Equals(RepositoryId.Owner, timeline.RepositoryId.Owner, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(RepositoryId.Name, timeline.RepositoryId.Name, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Merge sidecar attachment payload for a single timeline event.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
internal sealed record MergeAttachmentDto(
    [property: Key(0)] int EventIndex,
    [property: Key(1)] string TargetBranch);
