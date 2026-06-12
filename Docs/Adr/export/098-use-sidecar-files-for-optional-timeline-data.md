[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](097-use-atomic-file-transactions-for-multi-file-exports.md) | [Next](099-write-sidecars-through-dedicated-handlers.md)

## [098] Use Sidecar Files For Optional Timeline Data

*2026-06* | Status: accepted

**Context:**

Export is treated here as a full data pipeline, not a single function that writes a file. It covers history reading, enrichment, partitioning data into main artifacts and sidecars, and handling partial success and resume.

**Problem:**

Merge and pull-request enrichment can grow independently from the main timeline.
This area treats export as a stable data pipeline: from reading history, through enrichment, to persisting the main artifact and supporting files.

**Decision:**

Optional data is persisted in the `.gittrace.parts` directory as sidecars.
Export is treated as its own data-flow architecture, with dedicated backends, stages, persistence strategy, and handling for partial success.

**Rejected:**

- Packing all data into the main `.gittrace` file.
- Separate files without a defined relationship to the timeline.
- Treating export as one transaction without stages and without explicit partial states.
- Solving resilience concerns only at the CLI level instead of in the export pipeline itself.

**Consequences:**

The format is extensible, but readers must understand the relationship between the main file and the parts folder.
This improves resilience and scalability, but changes in the export pipeline must be evaluated for resume behavior, sidecars, and read compatibility.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](097-use-atomic-file-transactions-for-multi-file-exports.md) | [Next](099-write-sidecars-through-dedicated-handlers.md)
