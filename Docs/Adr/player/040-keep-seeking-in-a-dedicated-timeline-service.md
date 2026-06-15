[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](039-drive-playback-through-a-separate-transport.md) | [Next](041-model-playback-boundary-behavior-explicitly.md)

## [040] Keep Seeking In A Dedicated Timeline Service

*2026-06* | Status: accepted

**Context:**

`SeekableTimeline` ties together `IVirtualClock` and `IEventCursor`, handles position clamping, moves the cursor, and publishes progress. `TimelinePlayer` delegates `Seek` and `SeekRelative` to it without taking on the time and index repositioning details.

**Problem:**

Seeking changes two states at the same time: the virtual time position and the cursor position in the event stream. When those operations are scattered across the player or the calling layer, it is easy to create inconsistency between the reported position and the next event to be played.

**Decision:**

Seeking is maintained in a dedicated timeline service that treats the clock and cursor as one repositioning operation. The player consumes a ready made `ISeekable` contract instead of assembling seek logic itself.

**Rejected:**

- Modifying only the cursor without updating virtual time.
- Modifying only the virtual clock without repositioning the cursor.
- Implementing `Seek` directly in `TimelinePlayer`.
- Leaving position clamping and progress emission to the calling layer.

**Consequences:**

After a seek, the player has a consistent resume point and a stable progress contract. The tradeoff is that every change in seek semantics touches a central component used by the stepper, diagnostics, and later rendering.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](039-drive-playback-through-a-separate-transport.md) | [Next](041-model-playback-boundary-behavior-explicitly.md)
