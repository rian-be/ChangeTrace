[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](067-route-renderable-semantic-events-through-an-explicit-dispatch-table.md) | [Next](071-use-flyweight-backed-scene-nodes.md)

## [070] Represent Scene Relations Explicitly

*2026-05* | Status: accepted

**Context:**

This group of decisions concerns the logical layer of visual representation: how events become commands, commands become scene state, and scene state becomes snapshots and layout. It is a separate architecture inside the system, not a detail of a single renderer.

**Problem:**

Relations between nodes carry both domain and visual meaning.
Rendering needs its own operational model, because the transition from timeline data to scene state is not a simple one-to-one mapping.

**Decision:**

Scena uses jawnego modelu relacji.
This decision maintains a separate layer of scene state, commands, snapshots, and layout, with a clear boundary between event analysis and visual representation.

**Rejected:**

- Krawedzie jako anonimowe linie.
- Computing relations only in the GPU renderer.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Mixing translator, layout, and rendering-runtime responsibilities in one layer.

**Consequences:**

Scene state becomes richer, but the relation model must stay aligned with the translators.
The cost is a larger set of internal contracts, but the renderer stays modular and can evolve without breaking Core.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](067-route-renderable-semantic-events-through-an-explicit-dispatch-table.md) | [Next](071-use-flyweight-backed-scene-nodes.md)
