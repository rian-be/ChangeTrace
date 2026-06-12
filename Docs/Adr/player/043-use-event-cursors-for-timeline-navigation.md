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
These decisions establish a separate model of time, navigation, and diagnostics that rendering and debug tooling consume later.

**Rejected:**

- Iteracja ad hoc po listach events.
- Kazdy konsument z its ownm seekingm.
- Coupling the playback model directly to frame rate or the system clock.
- Splitting seeking, timekeeping, and boundary behavior across several inconsistent components.

**Consequences:**

Player ma stable contract, but cursor must uwzgledniac przypadki brzegowe.
This gives playback a stable contract and makes it testable without graphics, but any change to timekeeping or seeking affects several layers at once.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](042-use-a-virtual-clock-for-playback.md) | [Next](044-compose-players-from-specialized-playback-components.md)
