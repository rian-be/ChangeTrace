using ChangeTrace.GIt.Dto.Sidecars;

namespace ChangeTrace.GIt.Services.Sidecars;

/// <summary>
/// Debug snapshot for merge sidecar payloads.
/// </summary>
internal sealed class MergeSidecarDebugSnapshot
{
    /// <summary>
    /// Creates the snapshot.
    /// </summary>
    public MergeSidecarDebugSnapshot(
        DebugRepositorySnapshot? repository,
        IReadOnlyList<MergeAttachmentDebugSnapshot> attachments)
    {
        Repository = repository;
        Attachments = attachments;
        AttachmentCount = attachments.Count;
    }

    /// <summary>
    /// Gets the repository snapshot.
    /// </summary>
    public DebugRepositorySnapshot? Repository { get; }

    /// <summary>
    /// Gets the attachments.
    /// </summary>
    public IReadOnlyList<MergeAttachmentDebugSnapshot> Attachments { get; }

    /// <summary>
    /// Gets the attachment count.
    /// </summary>
    public int AttachmentCount { get; }
}

/// <summary>
/// Debug snapshot for a single merge attachment.
/// </summary>
internal sealed record MergeAttachmentDebugSnapshot(int EventIndex)
{
    /// <summary>
    /// Gets the semantic attachment type.
    /// </summary>
    public string Type => "Merge";

    /// <summary>
    /// Gets the target branch.
    /// </summary>
    public string? TargetBranch { get; init; }

    /// <summary>
    /// Creates a snapshot from a sidecar attachment.
    /// </summary>
    public static MergeAttachmentDebugSnapshot From(MergeAttachmentDto attachment)
        => new(attachment.EventIndex)
        {
            TargetBranch = attachment.TargetBranch
        };
}
