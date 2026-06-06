using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events.Info;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Events;

/// <summary>
/// Represents single trace event in repository timeline.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Holds core event data via <see cref="Core"/>.</item>
/// <item>Optionally references a <see cref="Commit"/>, <see cref="Branch"/>, <see cref="PullRequest"/>, or <see cref="Metadata"/>.</item>
/// <item>Supports computing relative times for playback via <see cref="RelativeTime"/>.</item>
/// <item>Provides convenience methods to create modified copies with updated pull request or metadata.</item>
/// </list>
/// </remarks>
internal readonly struct TraceEvent
{
    [Flags]
    private enum TraceEventParts : byte
    {
        None = 0,
        Commit = 1 << 0,
        Branch = 1 << 1,
        PullRequest = 1 << 2,
        Metadata = 1 << 3,
        RelativeTime = 1 << 4
    }

    private readonly CommitInfo _commit;
    private readonly BranchInfo _branch;
    private readonly PullRequestInfo _pullRequest;
    private readonly MetadataInfo _metadata;
    private readonly Duration _relativeTime;
    private readonly TraceEventParts _parts;

    public TraceEvent(
        TraceEventCore Core,
        CommitInfo? Commit = null,
        BranchInfo? Branch = null,
        PullRequestInfo? PullRequest = null,
        MetadataInfo? Metadata = null,
        Duration? RelativeTime = null)
    {
        this.Core = Core;
        _commit = Commit.GetValueOrDefault();
        _branch = Branch.GetValueOrDefault();
        _pullRequest = PullRequest.GetValueOrDefault();
        _metadata = Metadata.GetValueOrDefault();
        _relativeTime = RelativeTime.GetValueOrDefault();
        _parts = TraceEventParts.None;

        if (Commit.HasValue)
            _parts |= TraceEventParts.Commit;
        if (Branch.HasValue)
            _parts |= TraceEventParts.Branch;
        if (PullRequest.HasValue)
            _parts |= TraceEventParts.PullRequest;
        if (Metadata.HasValue)
            _parts |= TraceEventParts.Metadata;
        if (RelativeTime.HasValue)
            _parts |= TraceEventParts.RelativeTime;
    }

    private TraceEvent(
        TraceEventCore core,
        CommitInfo commit,
        BranchInfo branch,
        PullRequestInfo pullRequest,
        MetadataInfo metadata,
        Duration relativeTime,
        TraceEventParts parts)
    {
        Core = core;
        _commit = commit;
        _branch = branch;
        _pullRequest = pullRequest;
        _metadata = metadata;
        _relativeTime = relativeTime;
        _parts = parts;
    }

    public TraceEventCore Core { get; }

    public CommitInfo? Commit
        => (_parts & TraceEventParts.Commit) != 0 ? _commit : null;

    public BranchInfo? Branch
        => (_parts & TraceEventParts.Branch) != 0 ? _branch : null;

    public PullRequestInfo? PullRequest
        => (_parts & TraceEventParts.PullRequest) != 0 ? _pullRequest : null;

    public MetadataInfo? Metadata
        => (_parts & TraceEventParts.Metadata) != 0 ? _metadata : null;

    public Duration? RelativeTime
        => (_parts & TraceEventParts.RelativeTime) != 0 ? _relativeTime : null;

    /// <summary>
    /// Returns a copy of this <see cref="TraceEvent"/> with updated pull request info.
    /// </summary>
    /// <param name="number">The pull request number.</param>
    /// <param name="type">The type of pull request event.</param>
    /// <returns>A new <see cref="TraceEvent"/> with <see cref="PullRequest"/> set.</returns>
    public TraceEvent WithPullRequest(PullRequestNumber number, PullRequestEventType type)
        => new(
            Core,
            _commit,
            _branch,
            new PullRequestInfo(number, type),
            _metadata,
            _relativeTime,
            _parts | TraceEventParts.PullRequest);

    /// <summary>
    /// Computes the relative time from a base timestamp and optional scale factor.
    /// </summary>
    /// <param name="baseTime">The reference base timestamp.</param>
    /// <param name="scale">Scale factor to apply to the relative duration (default is 1.0).</param>
    /// <returns>A new <see cref="TraceEvent"/> with <see cref="RelativeTime"/> computed.</returns>
    public TraceEvent ComputeRelative(in Timestamp baseTime, double scale = 1.0)
        => new(
            Core,
            _commit,
            _branch,
            _pullRequest,
            _metadata,
            Core.Timestamp.Subtract(baseTime).Scale(scale),
            _parts | TraceEventParts.RelativeTime);

    /// <summary>
    /// Returns a copy of this <see cref="TraceEvent"/> with updated metadata.
    /// </summary>
    /// <param name="newMetadata">The metadata to set.</param>
    /// <returns>A new <see cref="TraceEvent"/> with <see cref="Metadata"/> set.</returns>
    public TraceEvent WithMetadata(in MetadataInfo newMetadata)
        => new(
            Core,
            _commit,
            _branch,
            _pullRequest,
            newMetadata,
            _relativeTime,
            _parts | TraceEventParts.Metadata);

    /// <summary>
    /// Gets the playback time in seconds, using <see cref="RelativeTime"/> if available, otherwise the core timestamp.
    /// </summary>
    public double TimeForPlayback
        => (_parts & TraceEventParts.RelativeTime) != 0
            ? _relativeTime.TotalSeconds
            : Core.Timestamp.UnixSeconds;

    /// <summary>
    /// Gets the primary target of the event: branch name, commit target, or core target.
    /// </summary>
    public string Target
        => (_parts & TraceEventParts.Branch) != 0
            ? _branch.Name.Value
            : Core.Target;
}
