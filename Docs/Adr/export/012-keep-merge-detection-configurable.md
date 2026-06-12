[ADR Home](../README.md) | [Category Index](./README.md) | [Next](014-centralize-export-orchestration-in-a-single-repository-exporter.md)

## [012] Keep Merge Detection Configurable

*2026-06* | Status: accepted

**Context:**

Export is treated here as a full data pipeline, not a single function that writes a file. It covers history reading, enrichment, partitioning data into main artifacts and sidecars, and handling partial success and resume.

**Problem:**

Merge detection can be expensive and can depend on the data available in a given repository.
This area treats export as a stable data pipeline: from reading history, through enrichment, to persisting the main artifact and supporting files.

**Decision:**

Export exposes configurable merge detection.
Export is treated as its own data-flow architecture, with dedicated backends, stages, persistence strategy, and handling for partial success.

**Rejected:**

- Zawsze wlaczone wykrywanie merge.
- Brak merge enrichment w eksporcie.
- Treating export as one transaction without stages and without explicit partial states.
- Solving resilience concerns only at the CLI level instead of in the export pipeline itself.

**Consequences:**

The operator controls export cost, but the options must be clear in the CLI.
This improves resilience and scalability, but changes in the export pipeline must be evaluated for resume behavior, sidecars, and read compatibility.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](014-centralize-export-orchestration-in-a-single-repository-exporter.md)
