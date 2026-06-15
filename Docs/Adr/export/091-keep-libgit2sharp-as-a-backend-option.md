[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](016-stream-repository-export-to-reduce-memory-pressure.md) | [Next](092-support-a-git-cli-history-backend.md)

## [091] Keep LibGit2Sharp As A Backend Option

*2026-06* | Status: accepted

**Context:**

The exporter now has more than one way to read repository history, but existing LibGit2Sharp based behavior is still known, tested, and useful. Replacing it outright would turn a backend expansion decision into a risky migration of the primary export path.

**Problem:**

Adding a new backend should not force abandonment of a working library based integration. The export pipeline needs a stable backend abstraction that can compare implementations and fall back when one path is less suitable for a repository or environment.

**Decision:**

LibGit2Sharp remains one supported history reading backend behind the common reader contract. Backend selection becomes an exporter concern instead of a hardcoded choice in repository reading code.

**Rejected:**

- Removing LibGit2Sharp immediately when a second backend appears.
- Keeping no backend abstraction at all.
- Treating backend choice as a compile time fork instead of a runtime export decision.
- Encoding backend choice only in benchmark or test code rather than in the exporter model.

**Consequences:**

The exporter can compare and retain known good behavior while broader backend support evolves. The tradeoff is that backend contracts, fallback rules, and behavioral parity now require ongoing validation.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](016-stream-repository-export-to-reduce-memory-pressure.md) | [Next](092-support-a-git-cli-history-backend.md)
