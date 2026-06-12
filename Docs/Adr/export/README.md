[ADR Home](../README.md)

# export

This category describes export as a pipeline that reads repository history, enriches it, and persists artifacts.

## Scope

Look here for Git backends, sidecars, checkpoints, atomic persistence, resume, and repository-to-artifact flow.

## Responsibility Boundaries

Export is not just serialization. It is a pipeline that coordinates history reading, enrichment, artifact partitioning, and partial-success behavior.

## How To Start Reading

Start here when changing repository reading, checkpointing, enrichment flow, or sidecars.

## ADR List

| ADR | Title |
| --- | --- |
| [012-keep-merge-detection-configurable.md](./012-keep-merge-detection-configurable.md) | Keep Merge Detection Configurable |
| [014-centralize-export-orchestration-in-a-single-repository-exporter.md](./014-centralize-export-orchestration-in-a-single-repository-exporter.md) | Centralize Export Orchestration In A Single Repository Exporter |
| [015-record-export-stage-completion-explicitly.md](./015-record-export-stage-completion-explicitly.md) | Record Export Stage Completion Explicitly |
| [016-stream-repository-export-to-reduce-memory-pressure.md](./016-stream-repository-export-to-reduce-memory-pressure.md) | Stream Repository Export To Reduce Memory Pressure |
| [091-keep-libgit2sharp-as-a-backend-option.md](./091-keep-libgit2sharp-as-a-backend-option.md) | Keep LibGit2Sharp As A Backend Option |
| [092-support-a-git-cli-history-backend.md](./092-support-a-git-cli-history-backend.md) | Support A Git CLI History Backend |
| [097-use-atomic-file-transactions-for-multi-file-exports.md](./097-use-atomic-file-transactions-for-multi-file-exports.md) | Use Atomic File Tranarections For Multi File Exports |
| [098-use-sidecar-files-for-optional-timeline-data.md](./098-use-sidecar-files-for-optional-timeline-data.md) | Use Sidecar Files For Optional Timeline Data |
| [099-write-sidecars-through-dedicated-handlers.md](./099-write-sidecars-through-dedicated-handlers.md) | Write Sidecars Through Dedicated Handlers |
| [100-use-checkpoints-for-resumable-export.md](./100-use-checkpoints-for-resumable-export.md) | Use Checkpoints For Resumable Export |
