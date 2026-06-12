[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](027-separate-cli-syntax-from-command-handling.md) | [Next](089-keep-interactive-cli-decisions-in-dedicated-prompt-components.md)

## [031] Use Nested Command Groups For Related CLI Workflows

*2026-02* | Status: accepted

**Context:**

These decisions keep the CLI as a predictable entry layer rather than a collection of accidental commands. The focus is on syntax consistency, stable entry points, and maintaining a thin orchestration layer.

**Problem:**

Auth, organizations, and workspaces have a natural hierarchy that should be visible in the CLI.
The concern is not only argument parsing, but preserving a stable execution model as the system grows.

**Decision:**

The CLI uses command groups such as `auth`, `org`, and `workspace`.
The CLI remains a thin orchestration layer: it defines how work enters the system, but it does not take ownership of export, auth, or rendering logic.

**Rejected:**

- One flat list of all commands.
- Packing multiple control flows into the options of one command.
- Extending the CLI through special cases instead of one command and handler convention.
- Treating commands as thin only in name while continuing to add application logic into them.

**Consequences:**

Command hierarchy reflects domain structure, but naming and aliases must stay consistent.
Commands can grow with the system without turning `Program.cs` into a monolith, but the command/handler convention has to be maintained consistently.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](027-separate-cli-syntax-from-command-handling.md) | [Next](089-keep-interactive-cli-decisions-in-dedicated-prompt-components.md)
