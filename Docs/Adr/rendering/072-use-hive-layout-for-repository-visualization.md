[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](071-use-flyweight-backed-scene-nodes.md) | [Next](073-advance-scene-systems-through-a-dedicated-frame-updater.md)

## [072] Use Hive Layout For Repository Visualization

*2026-05* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

A generic force-based layout does not always represent repository structure well.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

Repository visualization uses a modular Hive layout.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Force-directed layout jako docelowy model.
- Positioning nodes manually.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

The layout becomes more domain-specific, but it requires dedicated tests and benchmarks.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](071-use-flyweight-backed-scene-nodes.md) | [Next](073-advance-scene-systems-through-a-dedicated-frame-updater.md)
