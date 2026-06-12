# Architecture Decision Records

This directory stores ChangeTrace architectural decisions as ADRs. It is not a changelog and not a rewritten commit history. It is a curated set of technical decisions that explains why the system has its current shape and which constraints that imposes on future work.

ADRs are grouped by architecture area. Numbering is global, so a decision ID does not depend on the folder it lives in. Moving a file to another category does not change its number, and the number should be treated as a stable decision identifier.

## How To Read This Collection

Start with the category that matches the area you are changing:

- if the change affects the event model, timelines, or domain contracts, start in `core/`
- if it affects the `.gittrace` format, DTOs, or disk persistence, go to `persistence/` and `export/`
- if it affects authentication, workspaces, or hosting providers, read `auth workspace/` and `providers/`
- if it affects playback, semantic analysis, or rendering, read `player/`, `aggregation/`, `rendering/`, and `graphics/` in that order
- if it affects application composition and module boundaries, start in `composition/` and `cli/`

Each ADR contains:

- `Context`:
  the layer and architectural location of the decision
- `Problem`:
  the specific technical tension the decision resolves
- `Decision`:
  the chosen direction and responsibility boundary
- `Rejected`:
  realistic alternatives that were intentionally not selected
- `Consequences`:
  maintenance impact, constraints, side effects, and practical implications for future code

## How To Use ADRs During Changes

ADRs do not replace reading the code, but they reduce the cost of understanding decisions that have already been made. When you change a given area:

1. find the matching category
2. read the 2-4 closest ADRs, not just one
3. check whether the new change extends the current model or actually breaks it
4. if a decision is no longer true, add a new ADR instead of silently drifting away from the current direction

This collection is meant to preserve consistency across Core, export, providers, player, and rendering. In ChangeTrace, much of the cost of change comes from contracts between layers rather than from any single class.

## Category Map

The table below shows not only folder names, but also their role in the broader system flow.

| Folder | Scope |
| --- | --- |
| [foundation](./foundation/README.md) | Highest-level system and platform assumptions. The starting point for later decisions. |
| [core](./core/README.md) | Shared domain model: events, timelines, value objects, filters, and Core contracts. |
| [persistence](./persistence/README.md) | Durable data contracts: `.gittrace`, MessagePack, DTOs, mapping, and I/O. |
| [composition](./composition/README.md) | Application composition, dependency management, and runtime module boundaries. |
| [cli](./cli/README.md) | CLI entry layer: command structure, invocation flows, and CLI responsibilities. |
| [auth-workspace](./auth-workspace/README.md) | Local execution and configuration state: auth, profiles, workspaces, identities, and timeline metadata. |
| [providers](./providers/README.md) | Hosting-dependent behavior: provider detection, auth flows, enrichment, and fallbacks. |
| [export](./export/README.md) | Export architecture as a data pipeline: Git backends, sidecars, checkpoints, resume, and artifact persistence. |
| [player](./player/README.md) | Playback model: time, seeking, playback boundaries, and diagnostics. |
| [rendering](./rendering/README.md) | Logical visual representation: render commands, scene state, snapshots, layout, and render state. |
| [graphics](./graphics/README.md) | Execution-time graphics runtime: OpenTK, GPU, buffers, assets, shaders, input, and debug windows. |
| [aggregation](./aggregation/README.md) | Higher-level models built over TraceEvent: semantics, bundling, coupling, and pull request events. |
| [diagnostics](./diagnostics/README.md) | Logging rules and the distinction between failure paths and controlled degradation. |

## Relationships Between Categories

The most common architectural flow in ChangeTrace looks like this:

`foundation -> core -> providers/export -> persistence -> player/aggregation/rendering -> graphics`

This is not a strict dependency diagram of the project, but it is a useful map for reading decisions. In practice:

- `core/` defines what the system considers to be data and events
- `providers/` and `export/` define how that data is gathered and enriched
- `persistence/` defines how export output is stored
- `player/` and `aggregation/` prepare data for playback and interpretation
- `rendering/` and `graphics/` turn it into visual state and an executable rendering runtime

## When To Add A New ADR

A new ADR is worth adding when a change:

- shifts responsibility boundaries between layers
- introduces a new data contract or a new durable artifact
- changes the execution model of export, playback, or rendering
- adds a new provider specific behavior that no longer fits the current model
- replaces an earlier decision with a different conscious trade off

It is usually not worth adding an ADR for:

- an ordinary refactor without an architectural change in direction
- a cosmetic local API change limited to one file or class
- a documentation or test only adjustment that does not change system contracts
