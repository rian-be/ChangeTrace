[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](044-compose-players-from-specialized-playback-components.md) | [Next](046-expose-playback-diagnostics.md)

## [045] Derive Playback Duration From Active Repository Days

*2026-06* | Status: accepted

**Context:**

`TimelineDurationCalculator` does not base playback time on raw event count or the full date range. Playback duration is derived from the number of active days and then clamped to minimum and maximum session lengths.

**Problem:**

Repository histories are uneven: long periods without changes should not automatically stretch playback, and very dense days should not compress it into unreadable flicker. The player needs one time scale that stabilizes playback duration across different timelines.

**Decision:**

Playback duration is derived from the number of active days in repository history, with explicit `secondsPerDay`, `minDuration`, and `maxDuration` limits. Timeline normalization is performed relative to that target duration before the player starts.

**Rejected:**

- Playback duration proportional to the total number of events.
- Making duration proportional to the full calendar range, including inactive periods.
- No upper or lower bound for very short or very long histories.
- Choosing duration only in the renderer or HUD layer.

**Consequences:**

Different repositories get a more comparable playback session length, and seek plus diagnostics operate on one time scale. The downside is that playback time becomes a deliberate interpretation of history rather than a simple reflection of raw calendar time.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](044-compose-players-from-specialized-playback-components.md) | [Next](046-expose-playback-diagnostics.md)
