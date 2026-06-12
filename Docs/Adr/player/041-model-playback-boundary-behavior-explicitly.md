[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](040-keep-seeking-in-a-dedicated-timeline-service.md) | [Next](042-use-a-virtual-clock-for-playback.md)

## [041] Model Playback Boundary Behavior Explicitly

*2026-02* | Status: accepted

**Context:**

These decisions define the model of time and timeline navigation. The player is not meant to be just an iterator over an event list, but a separate layer capable of seeking, speed control, pausing, and diagnostics without depending on a specific rendering backend.

**Problem:**

After reaching the end of the timeline, the application can stop, loop, or reverse direction.
The player cannot be just a loop that advances an index, because playback has to be controllable, deterministic, and independent from the rendering backend.

**Decision:**

Boundary behavior is modeled through dedicated handlers.
These decisions establish a separate model of time, navigation, and diagnostics that rendering and debug tooling consume later.

**Rejected:**

- One hard coded reaction to the end of the timeline.
- Spreading boundary conditions across the player.
- Coupling the playback model directly to frame rate or the system clock.
- Splitting seeking, timekeeping, and boundary behavior across several inconsistent components.

**Consequences:**

Playback modes become extensible, but each one requires tests.
This gives playback a stable contract and makes it testable without graphics, but any change to timekeeping or seeking affects several layers at once.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](040-keep-seeking-in-a-dedicated-timeline-service.md) | [Next](042-use-a-virtual-clock-for-playback.md)
