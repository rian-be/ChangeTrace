[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](046-expose-playback-diagnostics.md) | [Next](048-ramp-playback-speed-toward-target-values.md)

## [047] Normalize Timeline Time Before Playback

*2026-02* | Status: accepted

**Context:**

These decisions define the model of time and timeline navigation. The player is not meant to be just an iterator over an event list, but a separate layer capable of seeking, speed control, pausing, and diagnostics without depending on a specific rendering backend.

**Problem:**

The date range in repository history does not map directly to playback pace.
The player cannot be just a loop that advances an index, because playback has to be controllable, deterministic, and independent from the rendering backend.

**Decision:**

The timeline is normalized into relative time before playback.
These decisions establish a separate model of time, navigation, and diagnostics that rendering and debug tooling consume later.

**Rejected:**

- Odtwarzanie po absolutnych datach.
- Scaling time in the renderer.
- Coupling the playback model directly to frame rate or the system clock.
- Splitting seeking, timekeeping, and boundary behavior across several inconsistent components.

**Consequences:**

The player behaves predictably, but normalization must stay aligned with seek behavior and speed control.
This gives playback a stable contract and makes it testable without graphics, but any change to timekeeping or seeking affects several layers at once.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](046-expose-playback-diagnostics.md) | [Next](048-ramp-playback-speed-toward-target-values.md)
