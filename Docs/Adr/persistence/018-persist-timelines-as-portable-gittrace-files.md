[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](017-optimize-gittrace-serialization-hot-paths.md) | [Next](019-use-streaming-serializers-for-gittrace-writes.md)

## [018] Persist Timelines As Portable gittrace Files

*2026-02* | Status: accepted

**Context:**

This decision concerns a durable data contract rather than a single persistence technique. The file format and the boundary between domain objects and persisted DTOs must survive not only a single export, but also later reads and code evolution.

**Problem:**

The operator must be able to export a repository and restore it later without rereading Git history.
The problem is not just the persistence file itself, but also format compatibility, the boundary between runtime models and persisted data, and the cost of reading on later runs.

**Decision:**

Trwalym artefaktem eksportu is plik .gittrace.
This decision keeps persistence as a separate layer with an explicit data contract and control over what is written to disk versus what remains a runtime implementation detail.

**Rejected:**

- Reconstructing from the repository on every run.
- Format zwiazany z oknem graficznym.
- Tightly coupling the file format to the current shape of runtime objects.
- Treating persistence as a side effect of export without a separate data contract.

**Consequences:**

Export becomes portable, but the file format requires careful evolution.
The result is a durable file contract that must be treated carefully when changing DTOs, formatters, and timeline repository behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](017-optimize-gittrace-serialization-hot-paths.md) | [Next](019-use-streaming-serializers-for-gittrace-writes.md)
