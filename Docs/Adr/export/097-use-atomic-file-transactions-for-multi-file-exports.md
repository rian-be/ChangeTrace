[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](092-support-a-git-cli-history-backend.md) | [Next](098-use-sidecar-files-for-optional-timeline-data.md)

## [097] Use Atomic File Transactions For Multi File Exports

*2026-06* | Status: accepted

**Context:**

Export can now write a main artifact plus sidecars and checkpoint state. Once output spans multiple files, interruption during persistence can leave the repository with a partially updated artifact set.

**Problem:**

A `.gittrace` export together with sidecars can leave inconsistent state when the process is interrupted. If writes happen directly to destination files, resume and load behavior have to reason about partially replaced artifacts.

**Decision:**

Multi file persistence uses an atomic transactional approach to files. Export prepares and commits related output as one persistence unit instead of exposing each artifact incrementally in its final location.

**Rejected:**

- Writing directly into destination files.
- Cleaning up only manually after a failure.
- Treating partial file sets as acceptable normal output.
- Delegating consistency guarantees entirely to callers or filesystem luck.

**Consequences:**

The risk of corrupted export state drops and resume logic sees cleaner persistence boundaries. The tradeoff is more complex I/O behavior and a stronger reliance on transaction style file management rules.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](092-support-a-git-cli-history-backend.md) | [Next](098-use-sidecar-files-for-optional-timeline-data.md)
