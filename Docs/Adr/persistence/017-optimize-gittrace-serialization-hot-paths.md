[ADR Home](../README.md) | [Category Index](./README.md) | [Next](018-persist-timelines-as-portable-gittrace-files.md)

## [017] Optimize gittrace Serialization Hot Paths

*2026-06* | Status: accepted

**Context:**

This decision concerns a durable data contract rather than a single persistence technique. The file format and the boundary between domain objects and persisted DTOs must survive not only a single export, but also later reads and code evolution.

**Problem:**

`.gittrace` serialization is critical for export time and file size.
The problem is not just the persistence file itself, but also format compatibility, the boundary between runtime models and persisted data, and the cost of reading on later runs.

**Decision:**

Formatters and DTOs are optimized for `.gittrace` persistence and reads.
This decision keeps persistence as a separate layer with an explicit data contract and control over what is written to disk versus what remains a runtime implementation detail.

**Rejected:**

- Changing the format without attempting optimization.
- Leaving serialization performance unmeasured.
- Tightly coupling the file format to the current shape of runtime objects.
- Treating persistence as a side effect of export without a separate data contract.

**Consequences:**

The format remains stable, but the optimizations must be controlled by compatibility tests.
The result is a durable file contract that must be treated carefully when changing DTOs, formatters, and timeline repository behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](018-persist-timelines-as-portable-gittrace-files.md)
