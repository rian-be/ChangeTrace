[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](063-aggregate-file-coupling-explicitly.md)

## [064] Use Semantic Aggregation Before Rendering

*2026-03* | Status: accepted

**Context:**

The rendering layer translates repository activity into scene state, but it is not the right place to infer every higher order concept from raw commit and file events. Semantic aggregation provides an intermediate boundary where low level history becomes domain relevant episodes that rendering can consume directly.

**Problem:**

The renderer needs higher level events than isolated file changes and raw commits. If every translator has to reconstruct semantic episodes independently, rendering becomes coupled to analytical policy and small interpretation changes become expensive to coordinate.

**Decision:**

Aggregators build semantic events before the rendering stage. The rendering pipeline consumes that prepared semantic stream instead of deriving higher order meaning on demand from raw timeline events.

**Rejected:**

- Every translator deriving semantics on its own.
- Extending `TraceEvent` with every analytical view.
- Passing only raw commit and file events into rendering and inferring the rest there.
- Treating semantic aggregation as a renderer local optimization instead of a reusable analytical stage.

**Consequences:**

Rendering receives richer, policy backed inputs and can stay focused on visual translation rather than historical inference. The cost is that aggregation becomes a hard behavioral boundary: changing semantic event rules can change rendering output even when no graphics code changes.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](063-aggregate-file-coupling-explicitly.md)
