[ADR Home](../README.md) | [Category Index](./README.md) | [Next](002-use-c-and-net-as-the-application-platform.md)

## [001] Establish Changetrace As A Local First CLI

*2026-02* | Status: accepted

**Context:**

These are decisions at the highest level of abstraction. They define not so much an implementation detail as the frame within which the remaining modules can exist and remain coherent. A change in this area usually forces coordinated changes in many parts of the repository.

**Problem:**

ChangeTrace needs a clear local entry point without depending on a fixed server side service.
This decision establishes the baseline assumptions of the system, and the rest of the repository is built around them.

**Decision:**

The primary system interface is a local CLI application, and execution state, exports, and playback remain on the local machine.
This layer does not resolve domain details yet, but it defines the environment and constraints within which the later modules evolve.

**Rejected:**

- A web application as the first interface.
- Requiring a remote ChangeTrace service in order to use the system.
- Spreading these assumptions across unrelated documents and modules.
- Deferring the platform decision until dependencies force it implicitly.

**Consequences:**

The system remains simple to run and local by nature, but storing data on the executing machine becomes an important contract.
Changes in this area usually have system wide impact rather than affecting only one functional slice.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](002-use-c-and-net-as-the-application-platform.md)
