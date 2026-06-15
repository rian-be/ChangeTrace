using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Info;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Translators;

namespace ChangeTrace.Benchmarks.Shared.Rendering;

/// <summary>
/// Shared deterministic benchmark inputs for CPU-side rendering benchmarks.
/// </summary>
internal static class RenderingBenchmarkData
{
    /// <summary>
    /// Creates synthetic commit-style trace events that exercise render aggregation and translation.
    /// </summary>
    public static TraceEvent[] CreateTraceEvents(int eventCount)
    {
        var events = new TraceEvent[eventCount];
        const long baseUnix = 1_700_000_000;

        var eventIndex = 0;
        var commitIndex = 0;

        while (eventIndex < eventCount)
        {
            var remaining = eventCount - eventIndex;
            var fileEvents = Math.Min(3, Math.Max(0, remaining - 1));
            var sha = CommitSha.Create($"{commitIndex + 0x1000000:x7}").Value;
            var actor = ActorName.Create($"actor-{commitIndex % 128}").Value;

            for (var fileIndex = 0; fileIndex < fileEvents; fileIndex++)
            {
                var timestamp = Timestamp.Create(baseUnix + eventIndex).Value;
                var filePath = $"src/module-{commitIndex % 64}/feature-{fileIndex}/file-{commitIndex}-{fileIndex}.cs";

                events[eventIndex++] = new TraceEvent(
                    new TraceEventCore(timestamp, actor, filePath),
                    Commit: new CommitInfo(sha, FileChangeKind.Modified));
            }

            if (eventIndex < eventCount)
            {
                var timestamp = Timestamp.Create(baseUnix + eventIndex).Value;
                var markerPath = $"src/module-{commitIndex % 64}/commit-{commitIndex}.cs";

                events[eventIndex++] = new TraceEvent(
                    new TraceEventCore(timestamp, actor, markerPath),
                    Commit: new CommitInfo(sha, FileChangeKind.Commit));
            }

            commitIndex++;
        }

        return events;
    }

    /// <summary>
    /// Creates synthetic semantic commit bundles for render translation or command dispatch benchmarks.
    /// </summary>
    public static CommitBundleEvent[] CreateCommitBundles(int eventCount)
    {
        var commitCount = Math.Max(1, eventCount / 4);
        var bundles = new CommitBundleEvent[commitCount];

        for (var i = 0; i < commitCount; i++)
        {
            var fileCount = 1 + i % 3;
            var files = new string[fileCount];

            for (var fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                files[fileIndex] = $"src/module-{i % 64}/feature-{fileIndex}/file-{i}-{fileIndex}.cs";
            }

            bundles[i] = new CommitBundleEvent(
                commitSha: $"commit-{i:x8}",
                actor: $"actor-{i % 128}",
                timestamp: i,
                files);
        }

        return bundles;
    }

    /// <summary>
    /// Creates a flattened render command batch translated from synthetic commit bundles.
    /// </summary>
    public static RenderCommand[] CreateRenderCommands(int eventCount)
    {
        var pipeline = TranslationPipeline.Default();
        var bundles = CreateCommitBundles(eventCount);
        var commands = new List<RenderCommand>(bundles.Length * 8);

        foreach (var bundle in bundles)
            commands.AddRange(pipeline.Translate(bundle));

        return commands.ToArray();
    }
}
