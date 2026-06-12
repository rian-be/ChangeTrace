[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](071-use-flyweight-backed-scene-nodes.md) | [Next](073-advance-scene-systems-through-a-dedicated-frame-updater.md)

## [072] Use Hive Layout For Repository Visualization

*2026-05* | Status: accepted

**Context:**

Generic force based placement does not necessarily reveal repository structure well enough for this domain. The visualization needs a layout model that can express repository centric grouping and navigation patterns rather than only generic graph aesthetics.

**Problem:**

A generic layout can preserve connectivity while still producing scenes that are hard to read as repository history. The renderer needs a layout model that reflects the kinds of clusters and navigation affordances expected from this domain.

**Decision:**

Repository visualization uses a Hive layout as a first class rendering policy. Layout rules are tailored to repository structure rather than inherited from a generic graph layout algorithm with minimal domain adaptation.

**Rejected:**

- Manual positioning of nodes.
- Treating generic force layout as the default for every repository view.
- Building visual behavior directly from the timeline model without a separate scene representation.
- Pushing domain specific layout rules down into backend renderers instead of keeping them in rendering policy.

**Consequences:**

The scene reflects repository specific structure more clearly and layout behavior can be tuned as a rendering concern. The tradeoff is that layout policy becomes more domain specific and therefore needs explicit tests and performance scrutiny of its own.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](071-use-flyweight-backed-scene-nodes.md) | [Next](073-advance-scene-systems-through-a-dedicated-frame-updater.md)
