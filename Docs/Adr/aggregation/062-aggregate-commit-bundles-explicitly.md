[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](054-model-pull-requests-as-semantic-events.md) | [Next](063-aggregate-file-coupling-explicitly.md)

## [062] Aggregate Commit Bundles Explicitly

*2026-06* | Status: accepted

**Context:**

Large repositories often produce bursts of commit activity that read as one episode of work rather than as dozens of isolated points. The raw timeline must preserve individual commits, but rendering and higher level analysis need a reusable grouped view for dense activity.

**Problem:**

Rendering every commit independently can flood the scene with short lived noise during rebases, automated churn, or concentrated feature work. If grouping happens only inside the renderer, the logic becomes hard to test and other consumers cannot reuse the same interpretation.

**Decision:**

Related commits are grouped by a dedicated aggregator that emits bundle level activity units on top of the base stream. Consumers can opt into those grouped events without losing access to the original commit level history.

**Rejected:**

- Always rendering every commit as a separate unit.
- Deferring grouping until the presentation layer.
- Collapsing commit bursts destructively inside the persisted base timeline.
- Hard coding one renderer specific bundling rule set with no reusable analytical representation.

**Consequences:**

Busy repositories become easier to read, and bundling policy can be tested outside graphics code. The cost is that grouping rules become an architectural contract: if they are too aggressive, meaningful commit level transitions disappear for every consumer using the bundled view.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](054-model-pull-requests-as-semantic-events.md) | [Next](063-aggregate-file-coupling-explicitly.md)
