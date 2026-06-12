[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](067-route-renderable-semantic-events-through-an-explicit-dispatch-table.md) | [Next](071-use-flyweight-backed-scene-nodes.md)

## [070] Represent Scene Relations Explicitly

*2026-05* | Status: accepted

**Context:**

Repository visualization is not just a set of isolated nodes. Branches, authors, files, and derived entities often need visible or queryable relationships that remain meaningful across layout, hover, and rendering passes.

**Problem:**

If scene relations are left implicit or reconstructed only inside GPU renderers, layout and interaction code have no stable way to reason about connectivity. That spreads relation semantics across multiple systems and makes it harder to preserve consistent behavior.

**Decision:**

Scene relations are represented explicitly in the rendering model. Relation structures live alongside scene entities so that layout, hover, bundling, and frame assembly can consume the same connectivity semantics.

**Rejected:**

- Computing relations only in the GPU renderer.
- Encoding every relation as an incidental property on unrelated node types.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Reconstructing connectivity heuristically in each scene subsystem.

**Consequences:**

Layout and interaction systems can share one model of connectivity instead of inferring it repeatedly. The tradeoff is that relation maintenance becomes part of scene lifecycle management and must stay aligned with translation output.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](067-route-renderable-semantic-events-through-an-explicit-dispatch-table.md) | [Next](071-use-flyweight-backed-scene-nodes.md)
