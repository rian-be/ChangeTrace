using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Info;
using ChangeTrace.Core.Models;
using ChangeTrace.Player.Playback;
using Xunit;

namespace ChangeTrace.Tests.Player.Playback;

public sealed class EventCursorTests
{
    [Fact]
    public void DrainForwardRange_ReturnsContiguousForwardBatch()
    {
        var cursor = new EventCursor(CreateEvents(8));

        var (startIndex, count) = cursor.DrainForwardRange(2.0);

        Assert.Equal(0, startIndex);
        Assert.Equal(3, count);
        Assert.Equal(3, cursor.Index);
        Assert.Equal(0, cursor.GetEventAt(startIndex).TimeForPlayback);
        Assert.Equal(2, cursor.GetEventAt(startIndex + count - 1).TimeForPlayback);
    }

    [Fact]
    public void DrainBackwardRange_ReturnsContiguousBackwardBatch()
    {
        var cursor = new EventCursor(CreateEvents(8));
        cursor.ResetToEnd();

        var (startIndex, count) = cursor.DrainBackwardRange(5.0);

        Assert.Equal(7, startIndex);
        Assert.Equal(3, count);
        Assert.Equal(4, cursor.Index);
        Assert.Equal(7, cursor.GetEventAt(startIndex).TimeForPlayback);
        Assert.Equal(5, cursor.GetEventAt(startIndex - count + 1).TimeForPlayback);
    }

    private static IReadOnlyList<TraceEvent> CreateEvents(int count)
    {
        var actor = ActorName.Create("player-test").Value;
        var events = new TraceEvent[count];

        for (var i = 0; i < count; i++)
        {
            var timestamp = Timestamp.Create(1_700_000_000 + i).Value;
            events[i] = new TraceEvent(
                new TraceEventCore(timestamp, actor, $"file-{i}.cs"),
                RelativeTime: new Duration(i));
        }

        return events;
    }
}
