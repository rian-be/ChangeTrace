[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](051-translate-trace-events-into-render-commands.md) | [Next](055-order-event-translators-by-explicit-priority.md)

## [052] Use Render Commands As Visual Intent

*2026-02* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

The renderer needs operations such as actor movement, branch labels, particles, and PR badges.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

Render commands describe visual intent independently from the graphics backend.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Making OpenGL calls directly from translators.
- One large visual state object without commands.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

Multiple outputs remain possible, but the command dispatcher has to be maintained carefully.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](051-translate-trace-events-into-render-commands.md) | [Next](055-order-event-translators-by-explicit-priority.md)
