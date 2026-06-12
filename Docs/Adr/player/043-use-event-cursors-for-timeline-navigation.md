[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](042-use-a-virtual-clock-for-playback.md) | [Next](044-compose-players-from-specialized-playback-components.md)

## [043] Use Event Cursors For Timeline Navigation

*2026-02* | Status: accepted

**Context:**

These decisions define the model of time and timeline navigation. The player is not meant to be just an iterator over an event list, but a separate layer capable of seeking, speed control, pausing, and diagnostics without depending on a specific rendering backend.

**Problem:**

Playback requires event traversal, seeking, and explicit boundary handling.
The player cannot be just a loop that advances an index, because playback has to be controllable, deterministic, and independent from the rendering backend.

**Decision:**

Timeline navigation goes through an event cursor and a seekable timeline.
This keeps navigation, boundary checks, and repositioning behavior inside a dedicated playback model that rendering and debug tooling can consume later.

**Rejected:**

- Ad hoc iteration over event lists.
- Letting each consumer implement its own seeking behavior.
- Coupling the playback model directly to frame rate or the system clock.
- Splitting seeking, timekeeping, and boundary behavior across several inconsistent components.

**Consequences:**

The player gets a stable navigation contract, but cursor behavior has to handle boundary conditions explicitly.
This gives playback a stable contract and makes it testable without graphics, but any change to timekeeping or seeking affects several layers at once.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](042-use-a-virtual-clock-for-playback.md) | [Next](044-compose-players-from-specialized-playback-components.md)
