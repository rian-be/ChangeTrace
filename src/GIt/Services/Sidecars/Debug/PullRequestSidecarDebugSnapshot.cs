using ChangeTrace.GIt.Dto.Sidecars;

namespace ChangeTrace.GIt.Services.Sidecars;

/// <summary>
/// Debug snapshot for pull request sidecar payloads.
/// </summary>
internal sealed class PullRequestSidecarDebugSnapshot
{
    /// <summary>
    /// Creates the snapshot.
    /// </summary>
    public PullRequestSidecarDebugSnapshot(
        DebugRepositorySnapshot? repository,
        IReadOnlyList<PullRequestAttachmentDebugSnapshot> attachments)
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
    public IReadOnlyList<PullRequestAttachmentDebugSnapshot> Attachments { get; }

    /// <summary>
    /// Gets the attachment count.
    /// </summary>
    public int AttachmentCount { get; }
}

/// <summary>
/// Debug snapshot for a single pull request attachment.
/// </summary>
internal sealed record PullRequestAttachmentDebugSnapshot(int EventIndex, int Number, string Type)
{
    /// <summary>
    /// Creates a snapshot from a sidecar attachment.
    /// </summary>
    public static PullRequestAttachmentDebugSnapshot From(PullRequestAttachmentDto attachment)
        => new(attachment.EventIndex, attachment.Number, attachment.Type);
}
