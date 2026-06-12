[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](092-support-a-git-cli-history-backend.md) | [Next](098-use-sidecar-files-for-optional-timeline-data.md)

## [097] Use Atomic File Transactions For Multi File Exports

*2026-06* | Status: accepted

**Context:**

Export is treated here as a full data pipeline, not a single function that writes a file. It covers history reading, enrichment, partitioning data into main artifacts and sidecars, and handling partial success and resume.

**Problem:**

A `.gittrace` export together with sidecars can leave inconsistent state when the process is interrupted.
This area treats export as a stable data pipeline: from reading history, through enrichment, to persisting the main artifact and supporting files.

**Decision:**

Multi-file persistence uses a transactional approach to files.
Export is treated as its own data-flow architecture, with dedicated backends, stages, persistence strategy, and handling for partial success.

**Rejected:**

- Writing directly into destination files.
- Cleaning up only manually after a failure.
- Treating export as one transaction without stages and without explicit partial states.
- Solving resilience concerns only at the CLI level instead of in the export pipeline itself.

**Consequences:**

The risk of corrupted exports decreases, but I/O becomes more complex.
This improves resilience and scalability, but changes in the export pipeline must be evaluated for resume behavior, sidecars, and read compatibility.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](092-support-a-git-cli-history-backend.md) | [Next](098-use-sidecar-files-for-optional-timeline-data.md)
