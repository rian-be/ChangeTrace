[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](091-keep-libgit2sharp-as-a-backend-option.md) | [Next](097-use-atomic-file-transactions-for-multi-file-exports.md)

## [092] Support A Git CLI History Backend

*2026-06* | Status: accepted

**Context:**

Some repositories and environments benefit from reading history through the Git CLI rather than a linked library backend. A CLI based path also gives the exporter another option for large repositories and platform specific scenarios.

**Problem:**

LibGit2Sharp should not be the only path for reading repository history. If one implementation becomes the sole backend, exporter scalability and platform behavior are constrained by that one integration model.

**Decision:**

A backend based on Git CLI and a parser for its output is added behind the common history reader contract. The exporter can select that backend when it is a better fit while keeping the rest of the export pipeline unchanged.

**Rejected:**

- One history backend for every repository and environment.
- Shelling out to Git with no tested parser contract.
- Treating Git CLI support as a benchmark only experiment outside production export.
- Building a backend path that bypasses the main exporter abstractions.

**Consequences:**

Export gains another scalable history reading option without splitting the rest of the pipeline. The tradeoff is that process execution, parser stability, and platform behavior become part of backend maintenance.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](091-keep-libgit2sharp-as-a-backend-option.md) | [Next](097-use-atomic-file-transactions-for-multi-file-exports.md)
