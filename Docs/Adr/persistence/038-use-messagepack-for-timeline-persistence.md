[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](037-keep-file-access-behind-file-manager-abstractions.md)

## [038] Use MessagePack For Timeline Persistence

*2026-02* | Status: accepted

**Context:**

This decision concerns a durable data contract rather than a single persistence technique. The file format and the boundary between domain objects and persisted DTOs must survive not only a single export, but also later reads and code evolution.

**Problem:**

The timeline can be large, so a text format would be expensive to persist and read.
The problem is not just the persistence file itself, but also format compatibility, the boundary between runtime models and persisted data, and the cost of reading on later runs.

**Decision:**

Timeline data is serialized in binary form through MessagePack.
This decision keeps persistence as a separate layer with an explicit data contract and control over what is written to disk versus what remains a runtime implementation detail.

**Rejected:** 

- JSON as the main runtime persistence format.
- A custom unproven binary format.
- Tightly coupling the file format to the current shape of runtime objects.
- Treating persistence as a side effect of export without a separate data contract.

**Consequences:**

Files stay compact and fast, but manual readability is lower and DTOs are required.
The result is a durable file contract that must be treated carefully when changing DTOs, formatters, and timeline repository behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](037-keep-file-access-behind-file-manager-abstractions.md)
