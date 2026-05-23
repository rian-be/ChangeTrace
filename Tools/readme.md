# ChangeTrace.Tools

Small infrastructure and tooling utilities used by ChangeTrace.

## Includes

* process execution
* file system helpers
* repository root discovery
* shared CLI/tooling utilities

---

## ProcessRunner

Runs external processes:

```csharp id="1v3r5z"
ProcessResult result = await processRunner.RunAsync(
    "git",
    ["status"]);
```

Supports:

* cancellation
* stderr capture
* working directory
* required execution
* command probing

---

## RepositoryRootFinder

Finds the repository root dynamically:

```csharp id="g0bnv1"
string root = repositoryRootFinder.Find();
```

Looks for a directory containing:

```text id="d9f7tw"
ChangeTrace.csproj
```

---

## FileSystem

Simple file system helpers:

```csharp id="3m7lqo"
fileSystem.EnsureParentDirectory(path);
```

---

## Goals

* lightweight
* minimal dependencies
* cross-platform
* easy to mock
* usable in CI and local tooling

---
# License

Internal ChangeTrace tooling infrastructure.
