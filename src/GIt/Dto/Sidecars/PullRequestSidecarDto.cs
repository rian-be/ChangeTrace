using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Dto.Common;
using MessagePack;
using MessagePack.Resolvers;

namespace ChangeTrace.GIt.Dto.Sidecars;

/// <summary>
/// Pull request sidecar payload for timeline exports.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
internal sealed record PullRequestSidecarDto
{
    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithCompression(MessagePackCompression.Lz4BlockArray)
        .WithResolver(StandardResolverAllowPrivate.Instance);

    [Key(0)] internal RepositoryIdDto? RepositoryId { get; init; }
    [Key(1)] internal required List<PullRequestAttachmentDto> Attachments { get; init; }

    /// <summary>
    /// Creates a sidecar payload from a timeline.
    /// </summary>
    internal static PullRequestSidecarDto FromDomain(Timeline timeline)
    {
        var events = timeline.EventsSpan;
        var attachments = new List<PullRequestAttachmentDto>();

        for (var index = 0; index < events.Length; index++)
        {
            var pullRequest = events[index].PullRequest;
            if (pullRequest is null)
                continue;

            attachments.Add(new PullRequestAttachmentDto(
                index,
                pullRequest.Value.Number.Value,
                pullRequest.Value.Type.ToString()));
        }

        return new PullRequestSidecarDto
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
    internal static PullRequestSidecarDto FromBytes(byte[] bytes)
        => MessagePackSerializer.Deserialize<PullRequestSidecarDto>(bytes, Options);

    /// <summary>
    /// Applies pull request attachments to a timeline.
    /// </summary>
    internal int ApplyTo(Timeline timeline)
    {
        if (!MatchesRepository(timeline))
            return 0;

        var applied = 0;
        foreach (var attachment in Attachments)
        {
            var type = attachment.ResolveType();
            if (type is null)
                continue;

            var numberResult = PullRequestNumber.Create(attachment.Number);
            if (numberResult.IsFailure)
                continue;

            if (!timeline.TryUpdateAt(
                    attachment.EventIndex,
                    evt => evt.WithPullRequest(numberResult.Value, type.Value)))
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
/// Pull request attachment payload for a single timeline event.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
internal sealed record PullRequestAttachmentDto(
    [property: Key(0)] int EventIndex,
    [property: Key(1)] int Number,
    [property: Key(2)] string Type)
{
    /// <summary>
    /// Resolves the pull request type.
    /// </summary>
    internal PullRequestEventType? ResolveType()
        => Enum.TryParse<PullRequestEventType>(Type, out var value) ? value : null;
}
