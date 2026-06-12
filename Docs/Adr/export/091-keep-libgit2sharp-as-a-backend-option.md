[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](016-stream-repository-export-to-reduce-memory-pressure.md) | [Next](092-support-a-git-cli-history-backend.md)

## [091] Keep LibGit2Sharp As A Backend Option

*2026-06* | Status: accepted

**Context:**

Export is treated here as a full data pipeline, not a single function that writes a file. It covers history reading, enrichment, partitioning data into main artifacts and sidecars, and handling partial success and resume.

**Problem:**

Zmiana backendu should not wymustc porzucenia dzialajacej integracji bibliotecznej.
This area treats export as a stable data pipeline: from reading history, through enrichment, to persisting the main artifact and supporting files.

**Decision:**

LibGit2Sharp remains one of the history-reading backends.
Export is treated as its own data-flow architecture, with dedicated backends, stages, persistence strategy, and handling for partial success.

**Rejected:**

- Natychmiastowe usuniecie LibGit2Sharp.
- Brak abstrakcji backendu.
- Treating export as one transaction without stages and without explicit partial states.
- Solving resilience concerns only at the CLI level instead of in the export pipeline itself.

**Consequences:**

Mozna porownywac backendi, but it is necesarery to utrzymywac ich contracts zgodne.
This improves resilience and scalability, but changes in the export pipeline must be evaluated for resume behavior, sidecars, and read compatibility.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](016-stream-repository-export-to-reduce-memory-pressure.md) | [Next](092-support-a-git-cli-history-backend.md)
