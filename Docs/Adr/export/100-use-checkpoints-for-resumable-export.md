[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](099-write-sidecars-through-dedicated-handlers.md)

## [100] Use Checkpoints For Resumable Export

*2026-06* | Status: accepted

**Context:**

Export is treated here as a full data pipeline, not a single function that writes a file. It covers history reading, enrichment, partitioning data into main artifacts and sidecars, and handling partial success and resume.

**Problem:**

Long exports should not restart from zero after interruption.
This area treats export as a stable data pipeline: from reading history, through enrichment, to persisting the main artifact and supporting files.

**Decision:**

Export maintains stage checkpoints and can resume work.
Export is treated as its own data-flow architecture, with dedicated backends, stages, persistence strategy, and handling for partial success.

**Rejected:**

- All-or-nothing export.
- Treating partial files as an implicit checkpoint.
- Treating export as one transaction without stages and without explicit partial states.
- Solving resilience concerns only at the CLI level instead of in the export pipeline itself.

**Consequences:**

Resume is mozliwe, but checkpointy staja sie dodatkowym stanem localm.
This improves resilience and scalability, but changes in the export pipeline must be evaluated for resume behavior, sidecars, and read compatibility.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](099-write-sidecars-through-dedicated-handlers.md)
