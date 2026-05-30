# Development

Docs for working on the codebase.

## Files

| File | Contents |
| --- | --- |
| [Setup](Setup.md) | Requirements and local run setup. |
| [Validation](Validation.md) | Build, tests, and quick checks. |
| [Project Structure](Project-Structure.md) | Where the main code lives. |
| [Debug](Debug.md) | `debdev` commands. |

## Minimum Before PR

```bash
dotnet build ChangeTrace.slnx
dotnet test Tests/ChangeTrace.Tests.csproj --no-build
```

The test project is top level sibling of `Tools/` and `Benchmarks/`.

Or:

```bash
task check
```
