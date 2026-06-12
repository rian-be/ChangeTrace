[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](012-keep-merge-detection-configurable.md) | [Next](015-record-export-stage-completion-explicitly.md)

## [014] Centralize Export Orchestration In A Single Repository Exporter

*2026-06* | Status: accepted

**Context:**

`RepositoryExporter` is not just another timeline persistence adapter. It is the central export orchestrator that ties together repository preparation, history reading, timeline building, enrichment, normalization, persistence, checkpoints, and the streaming variant.

**Problem:**

Export consists of multiple stages with different failure modes and different execution variants. When checkout, build, enrich, save, and resume are coordinated by several peer services or by the CLI, it becomes harder to maintain one model of progress, checkpoints, and the final semantics of success or degradation.

**Decision:**

Export orchestration remains concentrated in a single `RepositoryExporter`, which uses specialized dependencies but maintains the main workflow path. It decides the transitions between stages, the streaming variant, resume from checkpoints, and final timeline normalization.

**Rejected:**

- Coordinating export stages directly from the CLI layer.
- Separate, inconsistent orchestrators for export, export+save, and resume.
- Scattering checkpoint and enrichment responsibilities across helper services.
- Treating export as one simple `read -> save` call without an explicit staged workflow.

**Consequences:**

Export has a single main control point, which simplifies progress tracking, degradation handling, and resume. The drawback is that `RepositoryExporter` becomes an important coordination class and requires discipline to avoid growing into an implementation monolith.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](012-keep-merge-detection-configurable.md) | [Next](015-record-export-stage-completion-explicitly.md)
