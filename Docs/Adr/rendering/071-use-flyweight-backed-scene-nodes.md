[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](070-represent-scene-relations-explicitly.md) | [Next](072-use-hive-layout-for-repository-visualization.md)

## [071] Use Flyweight Backed Scene Nodes

*2026-05* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

A large number of nodes can cause redundant allocations and duplicated data.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

Scene nodes use flyweights where data can be shared.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Keeping a full data copy in every node.
- Globalny mutable cache bez contractu.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

Memory is controlled more effectively, but node-identity management becomes more complex.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](070-represent-scene-relations-explicitly.md) | [Next](072-use-hive-layout-for-repository-visualization.md)
