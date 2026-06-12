[ADR Home](../README.md) | [Category Index](./README.md) | [Next](031-use-nested-command-groups-for-user-workflows.md)

## [027] Separate CLI Syntax From Command Handling

*2026-02* | Status: accepted

**Context:**

The CLI defines the public entry points into export, auth, playback, and related workflows, but it is not the right place to absorb application logic. As command coverage grows, the boundary between parsing/orchestration and execution needs to stay explicit.

**Problem:**

If command definitions also implement execution logic, parsing concerns, dependency wiring, and application behavior collapse into the same layer. That makes commands harder to test, encourages duplication, and turns the CLI surface into the place where unrelated workflow rules accumulate.

**Decision:**

Each command has a separate handler that performs the work.
The CLI remains a thin orchestration layer: it defines how work enters the system, but it does not take ownership of export, auth, or rendering logic.

**Rejected:**

- Keeping application logic directly inside command definitions.
- One handler for many unrelated commands.
- Extending the CLI through special cases instead of one command and handler convention.
- Treating commands as thin only in name while continuing to add application logic into them.

**Consequences:**

The CLI stays easier to test and extend because syntax and execution evolve independently. The tradeoff is that command/handler pairs become an architectural convention that must be maintained consistently instead of bypassed for convenience.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](031-use-nested-command-groups-for-user-workflows.md)
