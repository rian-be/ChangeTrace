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
/// Handles merge sidecar persistence and hydration.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class MergeSidecarHandler(
    ILogger<MergeSidecarHandler> logger,
    IFileManager fileManager)
    : TimelineSidecarHandler<MergeSidecarDto>(logger, fileManager)
{
    /// <summary>
    /// Gets the sidecar file name.
    /// </summary>
    protected override string SidecarFileName => "merge.gittrace";

    /// <summary>
    /// Gets the display name used in logs.
    /// </summary>
    protected override string DisplayName => "Merge sidecar";

    /// <summary>
    /// Builds the sidecar payload from a timeline.
    /// </summary>
    protected override MergeSidecarDto BuildSidecar(Timeline timeline)
        => MergeSidecarDto.FromDomain(timeline);

    /// <summary>
    /// Gets whether the payload contains attachments.
    /// </summary>
    protected override bool HasAttachments(MergeSidecarDto sidecar)
        => sidecar.HasAttachments;

    /// <summary>
    /// Gets the attachment count for logs.
    /// </summary>
    protected override int GetAttachmentCount(MergeSidecarDto sidecar)
        => sidecar.Attachments.Count;

    /// <summary>
    /// Serializes the payload.
    /// </summary>
    protected override byte[] Serialize(MergeSidecarDto sidecar)
        => sidecar.ToBytes();

    /// <summary>
    /// Deserializes the payload.
    /// </summary>
    protected override MergeSidecarDto Deserialize(byte[] bytes)
        => MergeSidecarDto.FromBytes(bytes);

    /// <summary>
    /// Applies the payload to a timeline.
    /// </summary>
    protected override int ApplyToTimeline(MergeSidecarDto sidecar, Timeline timeline)
        => sidecar.ApplyTo(timeline);

    /// <summary>
    /// Creates the debug snapshot payload.
    /// </summary>
    protected override object CreateDebugSnapshot(MergeSidecarDto sidecar)
        => new MergeSidecarDebugSnapshot(
            sidecar.RepositoryId is { } repositoryId
                ? new DebugRepositorySnapshot(repositoryId.Owner, repositoryId.Name)
                : null,
            sidecar.Attachments.Select(MergeAttachmentDebugSnapshot.From).ToList());
}
