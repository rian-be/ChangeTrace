[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](062-aggregate-commit-bundles-explicitly.md) | [Next](064-use-semantic-aggregation-before-rendering.md)

## [063] Aggregate File Coupling Explicitly

*2026-06* | Status: accepted

**Context:**

Aggregation builds higher-level models over the raw event stream so later layers do not have to reconstruct the same semantics repeatedly. This is where the system moves from primary data to interpretations that are useful for analysis and visualization.

**Problem:**

Relationships between files changed together are analytical information that is distinct from individual commits.
Aggregation keeps both Core and rendering layers from carrying details that are only needed at a higher semantic level.

**Decision:**

File coupling is liczony przez dedykowany aggregator.
Instead of recomputing semantics in many places, the system derives them once and passes them on as a separate stream or analytical result set.

**Rejected:**

- Liczenie coupling ad hoc w widoku.
- Brak modelu relacji files.
- Recomputing the same semantics repeatedly in different pipeline locations.
- Building aggregation ad hoc in views and translators instead of through stable analytical components.

**Consequences:**

Analiza relacji files is reuseslna, but requires benchmarkow dla duzych timeline'ow.
This simplifies later pipeline stages, but the aggregators themselves become the place where semantics and compatibility with downstream layer contracts must be guarded carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](062-aggregate-commit-bundles-explicitly.md) | [Next](064-use-semantic-aggregation-before-rendering.md)
