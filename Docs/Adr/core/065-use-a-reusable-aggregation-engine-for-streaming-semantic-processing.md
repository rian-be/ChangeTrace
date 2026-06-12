[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](011-optimize-timeline-builder-hot-paths.md)

## [065] Use A Reuareble Aggregation Engine For Streaming Semantic Processing

*2026-06* | Status: accepted

**Context:**

The `Core.Aggregators` layer is not limited to individual classes such as `CommitBundlingAggregator` or `PullRequestAggregator`. Above them sits a shared `EventAggregationEngine<TIn>` that maintains a consistent model of streaming processing, batching, flushing, and disposal for semantic aggregators.

**Problem:**

Aggregators share a lifecycle: they accept events sequentially, sometimes operate on a single stream, sometimes on a batch, and require explicit flushing. If each aggregation stage implements that model on its own, it is easy for the semantics of `Process`, `Flush`, and `Dispose` to drift apart, which then leaks into export and rendering.

**Decision:**

Core maintains a reusable aggregation engine that orchestrates aggregator execution and separates streaming semantics from the specific domain logic of each aggregator. Aggregators define only semantic transformation, not their own execution framework.

**Rejected:**

- Manually calling `Process` and `Flush` for every aggregator at every call site.
- Separate aggregation loops for export, rendering, and other consumers.
- Embedding batching logic and disposal directly in individual aggregators.
- Treating aggregation as a one-off helper for one pipeline instead of a Core contract.

**Consequences:**

Aggregation semantics become more predictable and reusable across different system layers. The cost is an additional core abstraction that must remain simple enough not to hide important differences between domain aggregation stages.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](011-optimize-timeline-builder-hot-paths.md)
