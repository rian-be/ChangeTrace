[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](051-translate-trace-events-into-render-commands.md) | [Next](055-order-event-translators-by-explicit-priority.md)

## [052] Use Render Commands As Visual Intent

*2026-02* | Status: accepted

**Context:**

The renderer needs operations such as actor movement, branch labels, particles, and PR badges, but those operations should remain meaningful even if the concrete graphics backend changes. Render commands provide a backend neutral vocabulary for those visual intentions.

**Problem:**

Without a command layer, scene updates either become direct backend calls or one oversized mutable visual state object with weak boundaries. Both options make it harder to route intent cleanly through translation, buffering, and scene updates.

**Decision:**

Render commands describe visual intent independently from the graphics backend. Translators emit commands, scene systems consume them, and backend specific code only deals with prepared render state later in the pipeline.

**Rejected:**

- Making OpenGL calls directly from translators.
- One large visual state object without commands.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering runtime responsibilities in one layer.

**Consequences:**

Multiple outputs remain possible and the renderer keeps a stable internal vocabulary for scene updates. The tradeoff is that the command layer and its dispatcher become core contracts that must stay synchronized with scene behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](051-translate-trace-events-into-render-commands.md) | [Next](055-order-event-translators-by-explicit-priority.md)
