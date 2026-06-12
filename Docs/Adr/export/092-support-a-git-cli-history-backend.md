[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](091-keep-libgit2sharp-as-a-backend-option.md) | [Next](097-use-atomic-file-transactions-for-multi-file-exports.md)

## [092] Support A Git CLI History Backend

*2026-06* | Status: accepted

**Context:**

Export is treated here as a full data pipeline, not a single function that writes a file. It covers history reading, enrichment, partitioning data into main artifacts and sidecars, and handling partial success and resume.

**Problem:**

LibGit2Sharp should not be the only path for reading repository history.
This area treats export as a stable data pipeline: from reading history, through enrichment, to persisting the main artifact and supporting files.

**Decision:**

A backend based on Git CLI and a parser for its output is added.
Export is treated as its own data-flow architecture, with dedicated backends, stages, persistence strategy, and handling for partial success.

**Rejected:**

- Jeden backend historii.
- Shellowanie bez testow parsera.
- Treating export as one transaction without stages and without explicit partial states.
- Solving resilience concerns only at the CLI level instead of in the export pipeline itself.

**Consequences:**

Export gains an alternative path, but the parser and process runner require platform tests.
This improves resilience and scalability, but changes in the export pipeline must be evaluated for resume behavior, sidecars, and read compatibility.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](091-keep-libgit2sharp-as-a-backend-option.md) | [Next](097-use-atomic-file-transactions-for-multi-file-exports.md)
