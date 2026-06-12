[ADR Home](../README.md) | [Category Index](./README.md) | [Next](062-aggregate-commit-bundles-explicitly.md)

## [054] Model Pull Requests As Semantic Events

*2026-06* | Status: accepted

**Context:**

Aggregation builds higher-level models over the raw event stream so later layers do not have to reconstruct the same semantics repeatedly. This is where the system moves from primary data to interpretations that are useful for analysis and visualization.

**Problem:**

A pull request is an important collaboration event, not just a collection of branch and merge changes.
Aggregation keeps both Core and rendering layers from carrying details that are only needed at a higher semantic level.

**Decision:**

Pull requests are modeled as semantic events and can be rendered separately.
Instead of recomputing semantics in many places, the system derives them once and passes them on as a separate stream or analytical result set.

**Rejected:**

- Treating pull requests only as metadata enrichment.
- - Treating PR rendering as a renderer heuristic.
- Recomputing the same semantics repeatedly in different pipeline locations.
- Building aggregation ad hoc in views and translators instead of through stable analytical components.

**Consequences:**

Pull-request visualization becomes more coherent, but pull-request identity must remain stable between enrichment and rendering.
This simplifies later pipeline stages, but the aggregators themselves become the place where semantics and compatibility with downstream layer contracts must be guarded carefully.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](062-aggregate-commit-bundles-explicitly.md)
