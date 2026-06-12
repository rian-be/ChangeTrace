[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](043-use-event-cursors-for-timeline-navigation.md) | [Next](045-derive-playback-duration-from-active-repository-days.md)

## [044] Compose Players From Specialized Playback Components

*2026-06* | Status: accepted

**Context:**

The player in code is not a single object implementing the entire playback flow from start to finish. `TimelinePlayerFactory` composes instances from `VirtualClock`, `EventCursor`, `SeekableTimeline`, `PlaybackTransport`, and `TimelinePlayer` itself, while orchestration remains in the player layer.

**Problem:**

Full playback combines several different axes of responsibility: virtual time, navigation through events, transport, seeking, and diagnostics. Collapsing all of that into one class makes testing harder and blurs the boundary between time state, cursor state, and playback control.

**Decision:**

The player is composed from specialized components, each with its own contract. `TimelinePlayer` remains a coordinator rather than the place where every playback detail is implemented.

**Rejected:**

- A monolithic `TimelinePlayer` containing the clock, seeking, transport, and cursor.
- Constructing player dependencies directly in `Program.cs` or inside the graphics window.
- Dziedziczenie nextch wariantow playera instead of skladania zachowan z komponentow.
- Spreading player composition across many runtime layers without one factory boundary.

**Consequences:**

Playback ends up with more objects and more explicit contracts, but changes in seeking, the clock, or transport do not require rewriting the whole player. The factory becomes the place where timeline normalization, duration calculation, and default playback configuration have to stay aligned.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](043-use-event-cursors-for-timeline-navigation.md) | [Next](045-derive-playback-duration-from-active-repository-days.md)
