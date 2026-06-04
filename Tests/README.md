# ChangeTrace Tests

Regression tests live in `Tests/ChangeTrace.Tests.csproj`.

## Run

```bash
dotnet test Tests/ChangeTrace.Tests.csproj
```

More command variants are listed in [COMMANDS.md](COMMANDS.md).

The suite is designed to run without GPU access, OpenTK window creation, VSync,
or full rendering loops.

## Coverage

- Core value objects, validation, timeline normalization, and aggregation.
- CredentialTrace profile, workspace, auth, and temporary-filesystem persistence behavior.
- Player state transitions and boundary handling.
- Git DTO, provider helper, enricher, repository, and exporter behavior.
- Non-interactive CLI profile/workspace logic.
- CPU-side rendering state assembly and visibility rules.

Persistence tests use temporary or in-memory filesystem locations.
