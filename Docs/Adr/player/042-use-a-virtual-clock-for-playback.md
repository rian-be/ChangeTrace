[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](041-model-playback-boundary-behavior-explicitly.md) | [Next](043-use-event-cursors-for-timeline-navigation.md)

## [042] Use A Virtual Clock For Playback

*2026-02* | Status: accepted

**Context:**

These decisions define the model of time and timeline navigation. The player is not meant to be just an iterator over an event list, but a separate layer capable of seeking, speed control, pausing, and diagnostics without depending on a specific rendering backend.

**Problem:**

Playback must be deterministic and controllable, independent from system time.
The player cannot be just a loop that advances an index, because playback has to be controllable, deterministic, and independent from the rendering backend.

**Decision:**

The player uses a virtual clock.
These decisions establish a separate model of time, navigation, and diagnostics that rendering and debug tooling consume later.

**Rejected:**

- Using `DateTime.Now` as the source of truth for playback time.
- Controlling time from the renderer loop.
- Coupling the playback model directly to frame rate or the system clock.
- Splitting seeking, timekeeping, and boundary behavior across several inconsistent components.

**Consequences:**

Playback tests become simpler, and the presentation layer can pause and scrub time.
This gives playback a stable contract and makes it testable without graphics, but any change to timekeeping or seeking affects several layers at once.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](041-model-playback-boundary-behavior-explicitly.md) | [Next](043-use-event-cursors-for-timeline-navigation.md)
