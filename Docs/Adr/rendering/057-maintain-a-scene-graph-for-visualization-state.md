[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](056-keep-animation-as-a-subsystem.md) | [Next](058-render-from-immutable-scene-snapshots.md)

## [057] Maintain A Scene Graph For Visualization State

*2026-05* | Status: accepted

**Context:**

Repository visualization contains nodes, edges, labels, hoverable entities, and evolving layout state that persist across many frames. That state is richer than one immediate mode draw list and needs a structured in memory model between event translation and backend submission.

**Problem:**

If visual state is rebuilt from scratch for every event or hidden inside the player, the renderer loses continuity for layout, interaction, and multi frame effects. It also becomes hard to reason about identity and lifecycle for visual entities that outlive a single translation pass.

**Decision:**

Rendering maintains visualization state in a scene graph. Scene nodes, relationships, and subsystem owned state live in that graph until snapshot assembly extracts a renderable view.

**Rejected:**

- Rebuilding the scene from scratch for every event.
- Storing visual state inside the player.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Collapsing scene identity and render submission into one immediate mode structure.

**Consequences:**

The renderer can evolve scene state incrementally and support interaction, layout, and effects across frames. The tradeoff is lifecycle complexity: node identity, deletion, and synchronization now have to be managed explicitly inside the rendering layer.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](056-keep-animation-as-a-subsystem.md) | [Next](058-render-from-immutable-scene-snapshots.md)
