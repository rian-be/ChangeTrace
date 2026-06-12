[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](045-derive-playback-duration-from-active-repository-days.md) | [Next](047-normalize-timeline-time-before-playback.md)

## [046] Expose Playback DIagnostics

*2026-05* | Status: accepted

**Context:**

These decisions define the model of time and timeline navigation. The player is not meant to be just an iterator over an event list, but a separate layer capable of seeking, speed control, pausing, and diagnostics without depending on a specific rendering backend.

**Problem:**

Debugging the player requires visible time, cursor, and transport state.
The player cannot be just a loop that advances an index, because playback has to be controllable, deterministic, and independent from the rendering backend.

**Decision:**

Player publikuje snapshot diagnostyczny.
These decisions establish a separate model of time, navigation, and diagnostics that rendering and debug tooling consume later.

**Rejected:**

- Debugging only through logs.
- Reading private player state through the presentation layer.
- Coupling the playback model directly to frame rate or the system clock.
- Splitting seeking, timekeeping, and boundary behavior across several inconsistent components.

**Consequences:**

Diagnostics become more stable, but the snapshot must be updated when player state changes.
This gives playback a stable contract and makes it testable without graphics, but any change to timekeeping or seeking affects several layers at once.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](045-derive-playback-duration-from-active-repository-days.md) | [Next](047-normalize-timeline-time-before-playback.md)
