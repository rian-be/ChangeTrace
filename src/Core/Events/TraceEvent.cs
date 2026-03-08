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
internal readonly record struct TraceEvent(
    TraceEventCore Core,
    CommitInfo? Commit = null,
    BranchInfo? Branch = null,
    PullRequestInfo? PullRequest = null,
    MetadataInfo? Metadata = null,
    Duration? RelativeTime = null)
{
    /// <summary>
    /// Returns a copy of this <see cref="TraceEvent"/> with updated pull request info.
    /// </summary>
    /// <param name="number">The pull request number.</param>
    /// <param name="type">The type of pull request event.</param>
    /// <returns>A new <see cref="TraceEvent"/> with <see cref="PullRequest"/> set.</returns>
    public TraceEvent WithPullRequest(PullRequestNumber number, PullRequestEventType type)
        => this with { PullRequest = new PullRequestInfo(number, type) };

    /// <summary>
    /// Computes the relative time from a base timestamp and optional scale factor.
    /// </summary>
    /// <param name="baseTime">The reference base timestamp.</param>
    /// <param name="scale">Scale factor to apply to the relative duration (default is 1.0).</param>
    /// <returns>A new <see cref="TraceEvent"/> with <see cref="RelativeTime"/> computed.</returns>
    public TraceEvent ComputeRelative(Timestamp baseTime, double scale = 1.0)
        => this with { RelativeTime = Core.Timestamp.Subtract(baseTime).Scale(scale) };

    /// <summary>
    /// Returns a copy of this <see cref="TraceEvent"/> with updated metadata.
    /// </summary>
    /// <param name="newMetadata">The metadata to set.</param>
    /// <returns>A new <see cref="TraceEvent"/> with <see cref="Metadata"/> set.</returns>
    public TraceEvent WithMetadata(MetadataInfo newMetadata) => this with { Metadata = newMetadata };

    /// <summary>
    /// Gets the playback time in seconds, using <see cref="RelativeTime"/> if available, otherwise the core timestamp.
    /// </summary>
    public double TimeForPlayback => RelativeTime?.TotalSeconds ?? Core.Timestamp.UnixSeconds;

    /// <summary>
    /// Gets the primary target of the event: branch name, commit target, or core target.
    /// </summary>
    public string Target => Branch?.Name.Value ?? Commit?.Target ?? Core.Target;
}