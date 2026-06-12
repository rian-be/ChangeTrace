[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](014-centralize-export-orchestration-in-a-single-repository-exporter.md) | [Next](016-stream-repository-export-to-reduce-memory-pressure.md)

## [015] Record Export Stage Completion Explicitly

*2026-06* | Status: accepted

**Context:**

Resumable export only works when the exporter can tell which stages completed, which were skipped intentionally, and which never finished. File presence alone is not enough once export produces multiple artifacts and optional enrichment paths.

**Problem:**

Resume logic must know which stages are complete, including optional enrichment stages that were skipped. Without explicit stage records, interrupted runs become ambiguous and the exporter cannot tell whether to replay work, trust existing outputs, or clear partial state.

**Decision:**

Checkpoints persist each export stage and its status explicitly. Export records stage outcomes as part of its own state model instead of inferring them only from files on disk.

**Rejected:**

- Inferring state only from the presence of files.
- Re running every enrichment stage.
- Treating export as one opaque transaction with no stage visibility.
- Encoding stage progress only in logs or transient process memory.

**Consequences:**

Resume behavior becomes predictable and exporter state is easier to inspect and test. The tradeoff is that the stage model becomes a compatibility surface that future exporter changes must preserve or migrate deliberately.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](014-centralize-export-orchestration-in-a-single-repository-exporter.md) | [Next](016-stream-repository-export-to-reduce-memory-pressure.md)
