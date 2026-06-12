[ADR Home](../README.md)

# cli

This category describes decisions about CLI architecture and command workflows.

## Scope

Look here for command/handler separation, command groups, prompt components, terminal ergonomics, and the point where the CLI stops being a mere input layer.

## Responsibility Boundaries

The CLI should not absorb export, auth, or rendering logic. It is a thin orchestration and operator interaction layer.

## How To Start Reading

Start here when adding commands or changing interactive command flows.

## ADR List

| ADR | Title |
| --- | --- |
| [027-separate-cli-syntax-from-command-handling.md](./027-separate-cli-syntax-from-command-handling.md) | Separate CLI Syntax From Command Handling |
| [031-use-nested-command-groups-for-user-workflows.md](./031-use-nested-command-groups-for-user-workflows.md) | Use Nested Command Groups For Related CLI Workflows |
| [089-keep-interactive-cli-decisions-in-dedicated-prompt-components.md](./089-keep-interactive-cli-decisions-in-dedicated-prompt-components.md) | Keep Interactive CLI Decisions In Dedicated Prompt Components |
