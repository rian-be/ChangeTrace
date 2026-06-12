[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](014-centralize-export-orchestration-in-a-single-repository-exporter.md) | [Next](016-stream-repository-export-to-reduce-memory-pressure.md)

## [015] Record Export Stage Completion Explicitly

*2026-06* | Status: accepted

**Context:**

Export is treated here as a full data pipeline, not a single function that writes a file. It covers history reading, enrichment, partitioning data into main artifacts and sidecars, and handling partial success and resume.

**Problem:**

Resume logic must know which stages are complete, including optional enrichment stages that were skipped.
This area treats export as a stable data pipeline: from reading history, through enrichment, to persisting the main artifact and supporting files.

**Decision:**

Checkpoints persist each export stage and its status explicitly.
Export is treated as its own data-flow architecture, with dedicated backends, stages, persistence strategy, and handling for partial success.

**Rejected:**

- Inferring state only from the presence of files.
- Re-running every enrichment stage.
- Treating export as one transaction without stages and without explicit partial states.
- Solving resilience concerns only at the CLI level instead of in the export pipeline itself.

**Consequences:**

Resume behavior becomes predictable, but the stage model must remain compatible with future versions.
This improves resilience and scalability, but changes in the export pipeline must be evaluated for resume behavior, sidecars, and read compatibility.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](014-centralize-export-orchestration-in-a-single-repository-exporter.md) | [Next](016-stream-repository-export-to-reduce-memory-pressure.md)
