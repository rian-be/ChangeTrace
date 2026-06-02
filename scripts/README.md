# Scripts

This directory contains Python helpers used by repository workflows, release automation, and validation checks.

[Back to root README](../readme.md)

The purpose of this folder is simple: keep `.github/workflows/` focused on orchestration and move real logic into code that is easier to read and maintain.

## Structure

### `workflow_utils.py`

Shared helpers used across workflow scripts.

This file is for small reusable infrastructure such as:

- configuration loading
- webhook POST helpers
- `gh` command execution
- environment variable parsing

It should stay utility-focused. Workflow-specific behavior should live in the domain folders below.

### `release_post_publish/`

Scripts used by `.github/workflows/release-post-publish.yml`.

This folder handles the automation that runs after a release is published, including:

- preparing filtered release assets
- sending the main Discord release message
- publishing the changelog message

### `releases/`

Scripts used by release-related workflows such as:

- `.github/workflows/prerelease.yml`
- `.github/workflows/publish-attested.yml`
- `.github/workflows/release-drafter.yml`
- `.github/workflows/publish-artifacts.yml`

This folder is for shared release pipeline logic, including:

- prerelease metadata resolution
- artifact packaging
- draft release updates
- release asset uploads

### `checks/`

Scripts that validate repository invariants and fail CI when those invariants are broken.

This is the place for checks that have grown beyond a short shell command, such as shader validation and other repository-level validation tasks.

## Principles

Keep workflow files thin. They should define triggers, permissions, inputs, and job order, not act as long procedural programs.

Group scripts by responsibility. Shared release logic belongs in `releases/`. Post-release Discord logic belongs in `release_post_publish/`. Validation logic belongs in `checks/`.

Share infrastructure, not domain behavior. Common helpers belong in `workflow_utils.py`, while workflow-specific rules stay close to the workflow area that owns them.

Prefer extraction over shell growth. If a workflow step starts needing branching, parsing, validation, or structured config, move it here instead of extending an inline `run` block.

---