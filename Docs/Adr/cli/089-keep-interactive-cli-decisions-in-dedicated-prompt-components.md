[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](031-use-nested-command-groups-for-user-workflows.md)

## [089] Keep Interactive CLI Decisions In Dedicated Prompt Components

*2026-06* | Status: accepted

**Context:**

The CLI layer contains dedicated prompt components such as `EnrichmentPrompt`, `MergeDetectionPrompt`, and `ProviderPrompt`, instead of pushing interactive question logic directly into export handlers, auth handlers, and workspace workflows.

**Problem:**

Interactive choice of provider, enrichment, or merge detection is a separate responsibility from command execution. When a handler both asks for parameters, validates input, and triggers application logic, the boundary between CLI syntax and system behavior quickly becomes blurred.

**Decision:**

Interactive decisions stay in dedicated prompt layer components, and handlers consume already resolved options. The CLI keeps a separate layer for collecting execution time decisions instead of mixing that concern with export, auth, and provider aware execution logic.

**Rejected:**

- Asking interactive questions directly inside command handlers.
- Hiding every choice behind arguments only, with no interactive layer available.
- Duplicating prompt logic across multiple commands instead of maintaining shared components.
- Treating interactive input as a detail of provider or export layers.

**Consequences:**

The CLI remains clearer and interactive modes are easier to evolve without contaminating handlers. The drawback is that the prompt layer becomes its own contract, which must stay aligned with command options and workflow changes.

[ADR Home](../README.md) | [Category Index](./README.md) | [Previous](031-use-nested-command-groups-for-user-workflows.md)
