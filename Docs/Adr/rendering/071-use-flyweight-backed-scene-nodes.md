[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](070-represent-scene-relations-explicitly.md) | [Next](072-use-hive-layout-for-repository-visualization.md)

## [071] Use Flyweight Backed Scene Nodes

*2026-05* | Status: accepted

**Context:**

The rendering layer may hold large numbers of scene entities that share stable descriptive data while differing only in position, state, or transient render metadata. Duplicating the full payload in each node would waste memory and complicate updates.

**Problem:**

A large scene graph amplifies allocation pressure if every node owns a full copy of its descriptive state. That makes high entity count views more expensive and turns shared metadata updates into repeated node local mutations.

**Decision:**

Scene nodes use flyweight backed state where descriptive data can be shared safely. Node instances keep the per entity dynamic state they need, while reusable identity or metadata stays in shared backing structures.

**Rejected:**

- Keeping a full data copy in every node.
- Moving all node data into global bags with no typed node ownership.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Solving memory pressure only through backend level optimization after scene allocation already happened.

**Consequences:**

Scene memory use scales better in dense views and shared metadata changes become easier to centralize. The tradeoff is more indirection in node access and a stricter need to preserve flyweight identity and lifetime rules.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](070-represent-scene-relations-explicitly.md) | [Next](072-use-hive-layout-for-repository-visualization.md)
