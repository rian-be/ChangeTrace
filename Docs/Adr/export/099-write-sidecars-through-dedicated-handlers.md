[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](098-use-sidecar-files-for-optional-timeline-data.md) | [Next](100-use-checkpoints-for-resumable-export.md)

## [099] Write Sidecars Through Dedicated Handlers

*2026-06* | Status: accepted

**Context:**

Sidecars do not all share the same structure, lifecycle, or debugging needs. Pull request patches, merge artifacts, and related optional outputs have different persistence rules even though they participate in the same export.

**Problem:**

Each sidecar type has its own shape and write policy. If sidecar persistence is folded into the main timeline repository model, optional export concerns start to distort the core artifact path and sidecar evolution becomes hard to isolate.

**Decision:**

Sidecars are written through dedicated handlers. Each handler owns serialization and persistence rules for one sidecar type while the exporter orchestrates when those handlers run.

**Rejected:**

- One universal sidecar blob.
- Keeping sidecar logic inside the main repository timeline model.
- Treating sidecar writing as a generic file dump with no type specific contract.
- Spreading sidecar persistence logic across exporter stages and helper services.

**Consequences:**

The code becomes more modular and sidecar types can evolve independently. The tradeoff is that each new sidecar introduces another contract, another handler, and more targeted test coverage requirements.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](098-use-sidecar-files-for-optional-timeline-data.md) | [Next](100-use-checkpoints-for-resumable-export.md)
