[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](021-make-enrichment-optional-per-export.md) | [Next](023-treat-unavailable-enrichment-as-degraded-mode.md)

## [022] Skip Unavailable GitHub Pr Data Without Failing Export

*2026-06* | Status: accepted

**Context:**

GitHub pull request enrichment is useful but sits outside the core Git history path. The exporter can still build and persist a valid timeline when GitHub metadata is missing, stale, or inaccessible for a subset of commits.

**Problem:**

GitHub PR lookups can return `404` or otherwise fail even when repository history is readable and the base export is valid. Treating that case as a fatal export error couples optional enrichment to the success semantics of the main artifact.

**Decision:**

Unavailable GitHub PR data is skipped without failing the export. The exporter records the absence as an optional enrichment outcome and continues producing the main timeline plus any other artifacts that remain valid.

**Rejected:**

- Failing the export because the PR sidecar is missing.
- Retrying the same missing data case on every resume.
- Treating every GitHub API miss as evidence of exporter corruption.
- Hiding the skipped enrichment outcome entirely from diagnostics and resume state.

**Consequences:**

Repositories with unavailable PR data still produce `.gittrace`, which keeps the base export useful under partial API failure. The cost is that downstream consumers must tolerate missing PR sidecars and diagnostics must make the degraded outcome explicit.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](021-make-enrichment-optional-per-export.md) | [Next](023-treat-unavailable-github-pr-data-without-failing-export.md)
