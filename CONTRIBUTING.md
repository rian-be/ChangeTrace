# Contributing to ChangeTrace

Small and focused changes are easiest to review.

Please follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Before You Start

* check existing issues and pull requests
* keep the change focused on one thing
* say why the behavior changed when it matters

---

## Local Setup

Use the standard .NET build:

```bash
dotnet build
```

If the change touches rendering or UI behavior, run the app and check the flow once.

---

## Branches

* branch from `develop`
* use a short branch name
* keep unrelated cleanup out of the same branch

Example:

```bash
git checkout -b fix/auth-store
```

---

## Code Style

* keep the style already used in the file
* prefer small changes over broad rewrites
* match the naming and structure that already exists
* add abstractions only when they solve a real problem

---

## Pull Requests

Include:

* what changed
* why it changed
* any manual testing
* the linked issue, if there is one

Keep pull requests small when you can. They are easier to review and merge.

---

## Reporting Issues

If you find a bug, open an issue and include:

* what you expected
* what happened instead
* how to reproduce it
* your OS and .NET version if it matters

---

## License

By contributing to ChangeTrace, you agree that your contribution will be licensed under the same license as the project.
