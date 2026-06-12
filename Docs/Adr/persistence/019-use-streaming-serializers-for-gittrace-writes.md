[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](018-persist-timelines-as-portable-gittrace-files.md) | [Next](032-separate-persisted-dtos-from-domain-objects.md)

## [019] Use Streaming Serializers For gittrace Writes

*2026-06* | Status: accepted

**Context:**

This decision concerns a durable data contract rather than a single persistence technique. The file format and the boundary between domain objects and persisted DTOs must survive not only a single export, but also later reads and code evolution.

**Problem:**

Streaming export requires serializers that do not assume the full object graph is in memory.
The problem is not just the persistence file itself, but also format compatibility, the boundary between runtime models and persisted data, and the cost of reading on later runs.

**Decision:**

The serializer layer provides explicit streaming contracts for `.gittrace` writes.
This decision keeps persistence as a separate layer with an explicit data contract and control over what is written to disk versus what remains a runtime implementation detail.

**Rejected:**

- Serializers that only support complete in memory objects.
- Hard coding streaming persistence directly in the exporter.
- Tightly coupling the file format to the current shape of runtime objects.
- Treating persistence as a side effect of export without a separate data contract.

**Consequences:**

The serializer becomes more flexible, but it has more execution paths to test.
The result is a durable file contract that must be treated carefully when changing DTOs, formatters, and timeline repository behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](018-persist-timelines-as-portable-gittrace-files.md) | [Next](032-separate-persisted-dtos-from-domain-objects.md)
