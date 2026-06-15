using ChangeTrace.Core.Events;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Handlers;
using ChangeTrace.Player.Interfaces;
using Xunit;

namespace ChangeTrace.Tests.Player.Handlers;

/// <summary>Tests playback boundary handlers without running playback loop.</summary>
public sealed class BoundaryHandlerTests
{
    /// <summary>OnceBoundaryHandler stops playback and does not report loop.</summary>
    [Fact]
    public void OnceBoundaryHandler_Handle_StopsPlayback()
    {
        var handler = new OnceBoundaryHandler();

        var shouldStop = handler.Handle(
            new TestEventCursor(),
            new TestVirtualClock(),
            out var loopFired);

        Assert.True(shouldStop);
        Assert.False(loopFired);
    }

    /// <summary>LoopBoundaryHandler resets cursor and clock before continuing playback.</summary>
    [Fact]
    public void LoopBoundaryHandler_Handle_ResetsCursorAndClock()
    {
        var cursor = new TestEventCursor();
        var clock = new TestVirtualClock();
        var handler = new LoopBoundaryHandler();

        var shouldStop = handler.Handle(cursor, clock, out var loopFired);

        Assert.False(shouldStop);
        Assert.True(loopFired);
        Assert.Equal(1, cursor.ResetToStartCount);
        Assert.Equal(0, clock.SnappedPosition);
    }

    /// <summary>PingPongBoundaryHandler flips direction at each boundary hit.</summary>
    [Fact]
    public void PingPongBoundaryHandler_Handle_TogglesDirection()
    {
        var handler = new PingPongBoundaryHandler();

        handler.Handle(
            new TestEventCursor(),
            new TestVirtualClock(),
            out var firstLoop);
        var firstDirection = handler.Direction;

        handler.Handle(
            new TestEventCursor(),
            new TestVirtualClock(),
            out var secondLoop);

        Assert.True(firstLoop);
        Assert.True(secondLoop);
        Assert.Equal(PlaybackDirection.Backward, firstDirection);
        Assert.Equal(PlaybackDirection.Forward, handler.Direction);
    }

    /// <summary>Event cursor test double that records reset calls.</summary>
    private sealed class TestEventCursor : IEventCursor
    {
        /// <summary>Number of ResetToStart calls.</summary>
        public int ResetToStartCount { get; private set; }

        /// <summary>Current cursor index.</summary>
        public int Index => 0;

        /// <summary>Current event, unused by boundary handlers.</summary>
        public TraceEvent? CurrentEvent => null;

        /// <summary>Specific event lookup, unused by boundary handlers.</summary>
        public TraceEvent GetEventAt(int index) => default;

        /// <summary>First event, unused by boundary handlers.</summary>
        public TraceEvent? FirstEvent => null;

        /// <summary>Last event, unused by boundary handlers.</summary>
        public TraceEvent? LastEvent => null;

        /// <summary>Total event count, unused by boundary handlers.</summary>
        public int TotalEvents => 0;

        /// <summary>Start state, unused by boundary handlers.</summary>
        public bool AtStart => true;

        /// <summary>End state, unused by boundary handlers.</summary>
        public bool AtEnd => true;

        /// <summary>Returns no drained events.</summary>
        public IReadOnlyList<TraceEvent> DrainForward(double virtualNow) => [];

        /// <summary>Returns an empty forward range.</summary>
        public (int StartIndex, int Count) DrainForwardRange(double virtualNow) => (0, 0);

        /// <summary>Returns no drained events.</summary>
        public IReadOnlyList<TraceEvent> DrainBackward(double virtualNow) => [];

        /// <summary>Returns an empty backward range.</summary>
        public (int StartIndex, int Count) DrainBackwardRange(double virtualNow) => (0, 0);

        /// <summary>Reports no forward step.</summary>
        public (TraceEvent? Event, bool Moved) TryStepForward() => (null, false);

        /// <summary>Reports no backward step.</summary>
        public (TraceEvent? Event, bool Moved) TryStepBackward() => (null, false);

        /// <summary>Ignores seek requests.</summary>
        public void SeekTo(double virtualSeconds) { }

        /// <summary>Records reset-to-start calls.</summary>
        public void ResetToStart() => ResetToStartCount++;

        /// <summary>Ignores reset-to-end calls.</summary>
        public void ResetToEnd() { }
    }

    /// <summary>Virtual clock test double that records snapped position.</summary>
    private sealed class TestVirtualClock : IVirtualClock
    {
        /// <summary>Position passed to SnapPosition.</summary>
        public double? SnappedPosition { get; private set; }

        /// <summary>Wall time, unused by boundary handlers.</summary>
        public double WallNow => 0;

        /// <summary>Virtual time, unused by boundary handlers.</summary>
        public double VirtualNow => SnappedPosition ?? 0;

        /// <summary>Current speed, unused by boundary handlers.</summary>
        public double CurrentSpeed => 1;

        /// <summary>Target speed, unused by boundary handlers.</summary>
        public double TargetSpeed => 1;

        /// <summary>Acceleration, unused by boundary handlers.</summary>
        public double Acceleration { get; set; }

        /// <summary>Ramping state, unused by boundary handlers.</summary>
        public bool IsRamping => false;

        /// <summary>Ignores start requests.</summary>
        public void Start() { }

        /// <summary>Ignores reset requests.</summary>
        public void Reset() { }

        /// <summary>Ignores target speed requests.</summary>
        public void SetTargetSpeed(double target) { }

        /// <summary>Ignores speed snap requests.</summary>
        public void SnapSpeed(double speed) { }

        /// <summary>Records snapped virtual position.</summary>
        public void SnapPosition(double virtualPos) => SnappedPosition = virtualPos;

        /// <summary>Ignores reanchor requests.</summary>
        public void Reanchor() { }

        /// <summary>Ignores freeze requests.</summary>
        public void Freeze() { }
    }
}
