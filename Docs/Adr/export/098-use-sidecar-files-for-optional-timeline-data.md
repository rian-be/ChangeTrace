[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](097-use-atomic-file-transactions-for-multi-file-exports.md) | [Next](099-write-sidecars-through-dedicated-handlers.md)

## [098] Use Sidecar Files For Optional Timeline Data

*2026-06* | Status: accepted

**Context:**

Pull request and merge related information can evolve independently from the main timeline payload. Forcing every optional enrichment into the core `.gittrace` artifact would make the base format carry data that not every repository or export run can provide.

**Problem:**

Merge and pull request enrichment can grow independently from the main timeline. The exporter needs a way to persist optional data without making the base timeline format unstable or forcing optional host metadata to behave like mandatory core state.

**Decision:**

Optional export data is persisted in the `.gittrace.parts` directory as sidecars related to the main artifact. The base timeline remains loadable on its own, while optional parts can be discovered and rehydrated when available.

**Rejected:**

- Packing all data into the main `.gittrace` file.
- Separate files without a defined relationship to the timeline.
- Treating sidecar output as ad hoc debug artifacts rather than part of the export contract.
- Keeping optional host metadata only in memory after export.

**Consequences:**

The export format stays extensible without bloating the core artifact with optional host specific structures. The tradeoff is that readers, exporters, and diagnostics must all understand the relationship between the main artifact and its parts directory.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](097-use-atomic-file-transactions-for-multi-file-exports.md) | [Next](099-write-sidecars-through-dedicated-handlers.md)
