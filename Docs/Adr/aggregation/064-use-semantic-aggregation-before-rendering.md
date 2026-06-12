[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](063-aggregate-file-coupling-explicitly.md)

## [064] Use Semantic Aggregation Before Rendering

*2026-03* | Status: accepted

**Context:**

Aggregation builds higher-level models over the raw event stream so later layers do not have to reconstruct the same semantics repeatedly. This is where the system moves from primary data to interpretations that are useful for analysis and visualization.

**Problem:**

Renderer potrzebuje events wyzszego levelu niz pojedyncze changes files.
Aggregation keeps both Core and rendering layers from carrying details that are only needed at a higher semantic level.

**Decision:**

Aggregators build semantic events before the rendering stage.
Instead of recomputing semantics in many places, the system derives them once and passes them on as a separate stream or analytical result set.

**Rejected:**

- Every translator deriving semantics on its own.
- Extending `TraceEvent` with every analytical view.
- Recomputing the same semantics repeatedly in different pipeline locations.
- Building aggregation ad hoc in views and translators instead of through stable analytical components.

**Consequences:**

Rendering receives a richer stream, but aggregators become an important behavioral contract.
This simplifies later pipeline stages, but the aggregators themselves become the place where semantics and compatibility with downstream layer contracts must be guarded carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](063-aggregate-file-coupling-explicitly.md)
