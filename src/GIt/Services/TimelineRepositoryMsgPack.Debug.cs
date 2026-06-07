using System.Text.Json;
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
    private bool ShouldWriteDebugSnapshot(Timeline timeline)
        => (logger.IsEnabled(LogLevel.Debug) || logger.GetType().Name.StartsWith("NullLogger"))
           && timeline.Events.Count <= 50000;

    /// <summary>
    /// Writes a JSON debug snapshot alongside the saved timeline.
    /// </summary>
    private static async Task WriteDebugSnapshotAsync(
        Timeline timeline,
        string filePath,
        CancellationToken cancellationToken)
    {
        var snapshot = CreateDebugSnapshot(timeline);
        var json = JsonSerializer.Serialize(
            snapshot,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        await File.WriteAllTextAsync(filePath + ".debug.json", json, cancellationToken);
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
            var metadata = evt.Metadata;

            return new DebugTimelineEventSnapshot(
                evt.Core.Timestamp.UnixSeconds,
                evt.Core.Actor.Value,
                branch?.Name.Value,
                branch?.Type,
                commit?.Sha.Value,
                commit?.Type,
                metadata?.FilePath?.Value,
                metadata?.Metadata,
                evt.Target);
        }
    }
}
