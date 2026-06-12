[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](032-separate-persisted-dtos-from-domain-objects.md) | [Next](038-use-messagepack-for-timeline-persistence.md)

## [037] Keep File Access Behind File Manager Abstractions

*2026-02* | Status: accepted

**Context:**

This decision concerns a durable data contract rather than a single persistence technique. The file format and the boundary between domain objects and persisted DTOs must survive not only a single export, but also later reads and code evolution.

**Problem:**

Export, workspace storage, and token stores need I/O, but they should not duplicate file handling logic.
The problem is not just the persistence file itself, but also format compatibility, the boundary between runtime models and persisted data, and the cost of reading on later runs.

**Decision:** 

File operations are isolated behind file manager interfaces and services.
This decision keeps persistence as a separate layer with an explicit data contract and control over what is written to disk versus what remains a runtime implementation detail.

**Rejected:**

- Direct `File.WriteAllBytes` calls inside handlers.
- Hard coding shared paths in multiple places.
- Tightly coupling the file format to the current shape of runtime objects.
- Treating persistence as a side effect of export without a separate data contract.

**Consequences:**

I/O becomes easier to test, but the abstractions must stay simple.
The result is a durable file contract that must be treated carefully when changing DTOs, formatters, and timeline repository behavior.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](032-separate-persisted-dtos-from-domain-objects.md) | [Next](038-use-messagepack-for-timeline-persistence.md)
