[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](055-order-event-translators-by-explicit-priority.md) | [Next](057-maintain-a-scene-graph-for-visualization-state.md)

## [056] Keep Animation As A Subsystem

*2026-05* | Status: accepted

**Context:**

Animation, tweening, and particles evolve over frame time rather than over repository history alone. They depend on scene state and playback time, but they are not part of the domain model or the event translation layer.

**Problem:**

If animation behavior is mixed into translators or scene data structures directly, visual motion policy becomes hard to tune and easy to entangle with event semantics. The renderer needs one place that owns time based visual evolution independently from event interpretation.

**Decision:**

Animation is kept as a dedicated subsystem inside rendering. Scene updates hand it the state and timing context it needs, while translation and scene structure code stay focused on semantic meaning and stable entity state.

**Rejected:**

- Embedding tween and particle behavior directly in translators.
- Treating animation as backend only eye candy outside the rendering core.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing animation timing policy with unrelated frame assembly or camera code.

**Consequences:**

Animation behavior becomes easier to tune, replace, and test independently from event translation. The tradeoff is that another subsystem has to stay synchronized with frame timing, scene lifecycle, and playback state.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](055-order-event-translators-by-explicit-priority.md) | [Next](057-maintain-a-scene-graph-for-visualization-state.md)
