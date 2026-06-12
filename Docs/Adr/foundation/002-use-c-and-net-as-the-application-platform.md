[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](001-establish-changetrace-as-a-local-first-cli.md)

## [002] Use C And Net As The Application Platform

*2026-02* | Status: accepted

**Context:**

These are decisions at the highest level of abstraction. They define not so much an implementation detail as the frame within which the remaining modules can exist and remain coherent. A change in this area usually forces coordinated changes in many parts of the repository.

**Problem:**

The project requires one technology stack for the CLI, serialization, Git integration, tests, and later graphics work.
This decision establishes the baseline assumptions of the system, and the rest of the repository is built around them.

**Decision:**

The application is built as a C#/.NET project, with the .NET solution as the primary build and distribution mechanism.
This layer does not resolve domain details yet, but it defines the environment and constraints within which the later modules evolve.

**Rejected:**

- Scripts as the main application form.
- A multilingual core at project start.
- Spreading these assumptions across unrelated documents and modules.
- Deferring the platform decision until dependencies force it implicitly.

**Consequences:**

One runtime simplifies refactoring and testing, but library choice and distribution stay tied to the .NET ecosystem.
Changes in this area usually have system-wide impact rather than affecting only one functional slice.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](001-establish-changetrace-as-a-local-first-cli.md)
