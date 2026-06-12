[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](054-model-pull-requests-as-semantic-events.md) | [Next](063-aggregate-file-coupling-explicitly.md)

## [062] Aggregate Commit Bundles Explicitly

*2026-06* | Status: accepted

**Context:**

Aggregation builds higher-level models over the raw event stream so later layers do not have to reconstruct the same semantics repeatedly. This is where the system moves from primary data to interpretations that are useful for analysis and visualization.

**Problem:**

Visualizing individual commits can be too detailed for large repositories.
Aggregation keeps both Core and rendering layers from carrying details that are only needed at a higher semantic level.

**Decision:**

Related commits can be grouped by a dedicated aggregator.
Instead of recomputing semantics in many places, the system derives them once and passes them on as a separate stream or analytical result set.

**Rejected:**

- Always rendering every commit as a separate unit.
- Deferring grouping until the presentation layer.
- Recomputing the same semantics repeatedly in different pipeline locations.
- Building aggregation ad hoc in views and translators instead of through stable analytical components.

**Consequences:**

The scene can become easier to read, but bundling must not hide important events.
This simplifies later pipeline stages, but the aggregators themselves become the place where semantics and compatibility with downstream layer contracts must be guarded carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](054-model-pull-requests-as-semantic-events.md) | [Next](063-aggregate-file-coupling-explicitly.md)
