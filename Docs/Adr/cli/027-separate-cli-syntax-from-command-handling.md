[ADR Home](../README.md) | [Category Index](./README.md) | [Next](031-use-nested-command-groups-for-user-workflows.md)

## [027] Separate CLI Syntax From Command Handling

*2026-02* | Status: accepted

**Context:**

These decisions keep the CLI as a predictable entry layer rather than a collection of accidental commands. The focus is on syntax consistency, stable entry points, and maintaining a thin orchestration layer.

**Problem:**

CLI commands should define entry points rather than contain application logic.
The concern is not only argument parsing, but preserving a stable execution model as the system grows.

**Decision:**

Each command has a separate handler that performs the work.
The CLI remains a thin orchestration layer: it defines how work enters the system, but it does not take ownership of export, auth, or rendering logic.

**Rejected:**

- Logika biznesowa w definicjach komend.
- One handler for many unrelated commands.
- Extending the CLI through special cases instead of one command-and-handler convention.
- Treating commands as thin only in name while continuing to add application logic into them.

**Consequences:**

The CLI stays testable and extensible, but command/handler pairs must be maintained consistently.
Commands can grow with the system without turning `Program.cs` into a monolith, but the command/handler convention has to be maintained consistently.

[ADR Home](../README.md) | [Category Index](./README.md) | [Next](031-use-nested-command-groups-for-user-workflows.md)
