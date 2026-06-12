[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](019-use-streaming-serializers-for-gittrace-writes.md) | [Next](037-keep-file-access-behind-file-manager-abstractions.md)

## [032] Separate Persisted DTOs From Domain Objects

*2026-02* | Status: accepted

**Context:**

This decision concerns a durable data contract rather than a single persistence technique. The file format and the boundary between domain objects and persisted DTOs must survive not only a single export, but also later reads and code evolution.

**Problem:**

Obiekty domenowe can ewoluowac inaczej niz format utrwalony na dysku.
The problem is not just the persistence file itself, but also format compatibility, the boundary between runtime models and persisted data, and the cost of reading on later runs.

**Decision:**

Format .gittrace uses DTO oddzielonych od modeli domain.
This decision keeps persistence as a separate layer with an explicit data contract and control over what is written to disk versus what remains a runtime implementation detail.

**Rejected:**

- Serializacja modeli domain directly.
- Using one type for domain, transport, and persistence.
- Tightly coupling the file format to the current shape of runtime objects.
- Treating persistence as a side effect of export without a separate data contract.

**Consequences:**

The domain can be refactored, but DTO mappings require maintenance and tests.
The result is a durable file contract that must be treated carefully when changing DTOs, formatters, and timeline repository behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](019-use-streaming-serializers-for-gittrace-writes.md) | [Next](037-keep-file-access-behind-file-manager-abstractions.md)
