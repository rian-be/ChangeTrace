[ADR Home](../README.md) | [Category Index](./README.md) | [Next](040-keep-seeking-in-a-dedicated-timeline-service.md)

## [039] Drive Playback Through A Separate Transport

*2026-06* | Status: accepted

**Context:**

`PlaybackTransport` maintains the `Idle/Playing/Paused` state, emits ticks, and manages the timer. `TimelinePlayer` subscribes to those ticks and performs cursor draining and boundary handling on them, but it does not run the timing loop itself.

**Problem:**

Controlling playback flow and processing events are not the same responsibility. When one class both schedules ticks and executes playback logic on them, it becomes harder to isolate real time errors from player semantics errors.

**Decision:**

The tick loop and transport state transitions are separated into a dedicated transport. The player reacts to transport ticks, but it does not manage the timer or `Play/Pause/Stop` transitions directly.

**Rejected:**

- Embedding the `Timer` loop directly in `TimelinePlayer`.
- Updating playback only when the renderer or graphics window is invoked.
- Mixing transport state with boundary handling and seeking.
- Replacing explicit transport with ad hoc external polling.

**Consequences:**

Transport and player can be tested separately, and playback can stay outside the graphics layer. The cost is maintaining two state layers that must remain consistent: transport state and the logical state reported by the player.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](040-keep-seeking-in-a-dedicated-timeline-service.md)
