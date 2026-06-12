[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](055-order-event-translators-by-explicit-priority.md) | [Next](057-maintain-a-scene-graph-for-visualization-state.md)

## [056] Keep Animation As A Subsystem

*2026-05* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

Animations, tweens, and particles are visual behavior, not domain behavior.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

AnimationSystem is separatem subsystemem rendering.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Animacje w Core events.
- Efekty graficzne zaszyte w outputach.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

Effects become swappable and testable, but they must stay synchronized with the frame update.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](055-order-event-translators-by-explicit-priority.md) | [Next](057-maintain-a-scene-graph-for-visualization-state.md)
