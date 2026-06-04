# Test Commands

Run the full regression suite:

```bash
dotnet test Tests/ChangeTrace.Tests.csproj
```

Run without restore when packages are already restored:

```bash
dotnet test Tests/ChangeTrace.Tests.csproj --no-restore
```

Run without build after a successful build:

```bash
dotnet test Tests/ChangeTrace.Tests.csproj --no-build
```

Run selected areas:

```bash
dotnet test Tests/ChangeTrace.Tests.csproj --filter FullyQualifiedName~Core
dotnet test Tests/ChangeTrace.Tests.csproj --filter FullyQualifiedName~CredentialTrace
dotnet test Tests/ChangeTrace.Tests.csproj --filter FullyQualifiedName~Player
dotnet test Tests/ChangeTrace.Tests.csproj --filter FullyQualifiedName~Rendering
dotnet test Tests/ChangeTrace.Tests.csproj --filter FullyQualifiedName~GIt
```

Run a single test class:

```bash
dotnet test Tests/ChangeTrace.Tests.csproj --filter FullyQualifiedName~BoundaryHandlerTests
```
