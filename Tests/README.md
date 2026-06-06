# ChangeTrace Tests

Regression tests live in `Tests/ChangeTrace.Tests.csproj`.

## Structure

The test project is split by test purpose first:

- `Unit/` contains regular domain and module tests
- `Diagnostics/` contains heavier environment-specific investigation tests
- `Shared/` contains test doubles and shared helpers

Under `Unit/`, tests stay grouped by product area:

- `Unit/Core`
- `Unit/GIt`
- `Unit/Cli`
- `Unit/Player`
- `Unit/Rendering`
- `Unit/CredentialTrace`

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
