using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Dto.Sidecars;
using ChangeTrace.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Services.Sidecars;

/// <summary>
/// Handles pull request sidecar persistence and hydration.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class PullRequestSidecarHandler(
    ILogger<PullRequestSidecarHandler> logger,
    IFileManager fileManager)
    : TimelineSidecarHandler<PullRequestSidecarDto>(logger, fileManager)
{
    /// <summary>
    /// Gets the sidecar file name.
    /// </summary>
    protected override string SidecarFileName => "pullrequest.gittrace";

    /// <summary>
    /// Gets the display name used in logs.
    /// </summary>
    protected override string DisplayName => "Pull request sidecar";

    /// <summary>
    /// Builds the sidecar payload from a timeline.
    /// </summary>
    protected override PullRequestSidecarDto BuildSidecar(Timeline timeline)
        => PullRequestSidecarDto.FromDomain(timeline);

    /// <summary>
    /// Gets whether the payload contains attachments.
    /// </summary>
    protected override bool HasAttachments(PullRequestSidecarDto sidecar)
        => sidecar.HasAttachments;

    /// <summary>
    /// Gets the attachment count for logs.
    /// </summary>
    protected override int GetAttachmentCount(PullRequestSidecarDto sidecar)
        => sidecar.Attachments.Count;

    /// <summary>
    /// Serializes the payload.
    /// </summary>
    protected override byte[] Serialize(PullRequestSidecarDto sidecar)
        => sidecar.ToBytes();

    /// <summary>
    /// Deserializes the payload.
    /// </summary>
    protected override PullRequestSidecarDto Deserialize(byte[] bytes)
        => PullRequestSidecarDto.FromBytes(bytes);

    /// <summary>
    /// Applies the payload to a timeline.
    /// </summary>
    protected override int ApplyToTimeline(PullRequestSidecarDto sidecar, Timeline timeline)
        => sidecar.ApplyTo(timeline);

    /// <summary>
    /// Creates the debug snapshot payload.
    /// </summary>
    protected override object CreateDebugSnapshot(PullRequestSidecarDto sidecar)
        => new PullRequestSidecarDebugSnapshot(
            sidecar.RepositoryId is { } repositoryId
                ? new DebugRepositorySnapshot(repositoryId.Owner, repositoryId.Name)
                : null,
            sidecar.Attachments.Select(PullRequestAttachmentDebugSnapshot.From).ToList());
}
