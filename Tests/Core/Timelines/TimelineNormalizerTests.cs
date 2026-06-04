using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using Xunit;

namespace ChangeTrace.Tests.Core.Timelines;

/// <summary>Tests timeline ordering and relative-time normalization.</summary>
public sealed class TimelineNormalizerTests
{
    /// <summary>Normalize sorts events by timestamp and maps them into the requested duration.</summary>
    [Fact]
    public void Normalize_SortsEventsAndComputesRelativeTime()
    {
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        timeline.AddEvent(CreateCommit(300));
        timeline.AddEvent(CreateCommit(100));
        timeline.AddEvent(CreateCommit(200));

        var result = TimelineNormalizer.Normalize(timeline, targetDurationSeconds: 20);

        Assert.True(result.IsSuccess);
        Assert.Equal([100, 200, 300], timeline.Events.Select(evt => evt.Core.Timestamp.UnixSeconds).ToArray());
        Assert.Equal([0, 10, 20], timeline.Events.Select(evt => evt.RelativeTime?.TotalSeconds).ToArray());
    }

    /// <summary>Normalize returns a failure result for a timeline without events.</summary>
    [Fact]
    public void Normalize_FailsForEmptyTimeline()
    {
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);

        var result = TimelineNormalizer.Normalize(timeline);

        Assert.True(result.IsFailure);
    }

    /// <summary>Creates a commit event at a fixed Unix timestamp for normalization tests.</summary>
    private static TraceEvent CreateCommit(long timestamp)
        => TraceEventFactory.Commit(
            Timestamp.Create(timestamp).Value,
            ActorName.Create("rian").Value,
            CommitSha.Create($"{timestamp:x40}").Value,
            $"Commit {timestamp}");
}
