using System.Text.Json;
using System.Text.Json.Serialization;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Timelines;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Debug snapshot output for timeline saves.
/// </summary>
internal sealed partial class TimelineRepositoryMsgPack
{
    /// <summary>
    /// Enables debug snapshot writes for small timelines.
    /// </summary>
    private string? GetDebugSnapshotSkipReason(Timeline timeline)
    {
        if (timeline.Events.Count > 50000)
            return $"timeline has {timeline.Events.Count} events";

        if (logger.IsEnabled(LogLevel.Debug) || logger.GetType().Name.StartsWith("NullLogger"))
            return null;

        return "logger is not configured for Debug level";
    }

    /// <summary>
    /// Writes a JSON debug snapshot alongside the saved timeline.
    /// </summary>
    private static async Task WriteDebugSnapshotAsync(
        AtomicFileTransaction transaction,
        Timeline timeline,
        string filePath,
        CancellationToken cancellationToken)
    {
        var snapshot = CreateDebugSnapshot(timeline);
        var json = JsonSerializer.Serialize(
            snapshot,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        await transaction.WriteBytesAsync(GetDebugSnapshotPath(filePath), bytes, cancellationToken);
    }

    /// <summary>
    /// Creates the debug snapshot payload.
    /// </summary>
    private static DebugTimelineSnapshot CreateDebugSnapshot(Timeline timeline)
    {
        var repository = timeline.RepositoryId is { } repositoryId
            ? new DebugRepositorySnapshot(repositoryId.Owner, repositoryId.Name)
            : null;

        return new DebugTimelineSnapshot(
            repository,
            timeline.Events.Count,
            timeline.Events.Select(DebugTimelineEventSnapshot.From).ToList());
    }

    /// <summary>
    /// Timeline debug snapshot root.
    /// </summary>
    private sealed class DebugTimelineSnapshot(
        DebugRepositorySnapshot? repository,
        int eventCount,
        IReadOnlyList<DebugTimelineEventSnapshot> events)
    {
        public DebugRepositorySnapshot? Repository { get; } = repository;
        public int EventCount { get; } = eventCount;
        public IReadOnlyList<DebugTimelineEventSnapshot> Events { get; } = events;
    }

    /// <summary>
    /// Repository debug snapshot.
    /// </summary>
    private sealed class DebugRepositorySnapshot(string owner, string name)
    {
        public string Owner { get; } = owner;
        public string Name { get; } = name;
    }

    /// <summary>
    /// Timeline event debug snapshot.
    /// </summary>
    private sealed class DebugTimelineEventSnapshot(
        long timestamp,
        string? actor,
        string? branch,
        object? branchType,
        string? commitSha,
        object? commitType,
        int? pullRequestNumber,
        object? pullRequestType,
        string? filePath,
        string? metadataMessage,
        string target)
    {
        public long Timestamp { get; } = timestamp;
        public string? Actor { get; } = actor;
        public string? Branch { get; } = branch;
        public object? BranchType { get; } = branchType;
        public string? CommitSha { get; } = commitSha;
        public object? CommitType { get; } = commitType;
        public int? PullRequestNumber { get; } = pullRequestNumber;
        public object? PullRequestType { get; } = pullRequestType;
        public string? FilePath { get; } = filePath;
        public string? MetadataMessage { get; } = metadataMessage;
        public string Target { get; } = target;

        /// <summary>
        /// Creates a snapshot from a timeline event.
        /// </summary>
        public static DebugTimelineEventSnapshot From(TraceEvent evt)
        {
            var branch = evt.Branch;
            var commit = evt.Commit;
            var pullRequest = evt.PullRequest;
            var metadata = evt.Metadata;

            return new DebugTimelineEventSnapshot(
                evt.Core.Timestamp.UnixSeconds,
                evt.Core.Actor.Value,
                branch?.Name.Value,
                branch?.Type,
                commit?.Sha.Value,
                commit?.Type,
                pullRequest?.Number.Value,
                pullRequest?.Type,
                metadata?.FilePath?.Value,
                metadata?.Metadata,
                evt.Target);
        }
    }

    /// <summary>
    /// Gets the debug snapshot path for the main timeline file.
    /// </summary>
    private static string GetDebugSnapshotPath(string filePath)
        => filePath + ".debug.json";
}
