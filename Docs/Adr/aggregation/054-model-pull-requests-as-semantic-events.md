[ADR Home](../README.md) | [Category Index](./README.md) | [Next](062-aggregate-commit-bundles-explicitly.md)

## [054] Model Pull Requests As Semantic Events

*2026-06* | Status: accepted

**Context:**

Pull request data enters the system as enrichment attached to commits and sidecars, but later consumers do not work well against provider payloads directly. Rendering and analysis need one stable representation for review activity, merge intent, and PR lifecycle.

**Problem:**

A pull request spans multiple commits and often multiple branches, but raw timeline events expose only the low level pieces. If each consumer has to reconstruct PR identity and lifecycle independently, the repository accumulates duplicate provider specific logic and inconsistent behavior.

**Decision:**

Pull requests are elevated into dedicated semantic events produced by aggregation. The aggregator correlates enriched commit data into a PR level representation that downstream consumers can render, filter, and analyze without redoing provider specific correlation work.

**Rejected:**

- Treating pull requests only as commit attached metadata enrichment.
- Letting the renderer infer pull request structure heuristically from commit sequences.
- Repeating provider specific PR correlation logic in each analytical consumer.
- Extending the base event model until it directly carries provider review semantics.

**Consequences:**

Pull request visualization and analysis now share one semantic contract instead of several local heuristics. The tradeoff is that PR identity, merge semantics, and enrichment compatibility have to remain stable at the aggregation boundary.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](062-aggregate-commit-bundles-explicitly.md)
