[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](062-aggregate-commit-bundles-explicitly.md) | [Next](064-use-semantic-aggregation-before-rendering.md)

## [063] Aggregate File Coupling Explicitly

*2026-06* | Status: accepted

**Context:**

File coupling is not a first class concept in the raw timeline. It has to be derived by correlating files that repeatedly change together across many commits, and the resulting relationship is useful to both analytics and visualization.

**Problem:**

Relationships between files changed together are analytical information that is distinct from individual commits. If coupling is computed ad hoc in one view or renderer, each consumer pays the scan cost separately and may disagree about what counts as a meaningful relationship.

**Decision:**

File coupling is computed by a dedicated aggregator that scans the event stream and emits reusable co change relationships. Consumers work with an already derived coupling model instead of rebuilding file relationship statistics on their own.

**Rejected:**

- Computing coupling ad hoc in a single view.
- Leaving file relationships implicit and never modeling them explicitly.
- Rebuilding co change statistics inside rendering or export consumers whenever they need them.
- Storing coupling only as mutable renderer state with no analytical representation outside graphics.

**Consequences:**

File relationship analysis becomes reusable across features and can evolve independently from views. The downside is that coupling computation can be expensive on large histories, so thresholds, memory behavior, and benchmark coverage become part of the aggregator contract.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](062-aggregate-commit-bundles-explicitly.md) | [Next](064-use-semantic-aggregation-before-rendering.md)
