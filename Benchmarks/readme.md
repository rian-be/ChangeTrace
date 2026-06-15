# ChangeTrace Benchmarks

ChangeTrace Benchmarks contains repeatable BenchmarkDotNet scenarios for performance sensitive CPU paths.

The benchmark project exists to measure performance sensitive CPU paths before deeper optimization work. It covers Core timeline construction, timeline serialization, semantic aggregation, Git repository reading, export orchestration, and the CPU side render pipeline before OpenGL/GPU execution.

> [!IMPORTANT]
> These benchmarks are for local performance investigation and regression checks. They are not part of the normal application runtime.

> [!WARNING]
> Benchmark results depend on hardware, runtime version, operating system scheduling, power mode, and background processes. Compare results from the same machine and environment when possible.

Use it when you want a repeatable way to check whether Core, Git, export, player, or render pipeline changes affect CPU time, managed allocations, or scaling with larger timelines.

The usual flow is simple:

- run a focused benchmark group
- compare execution time and allocations
- inspect generated BenchmarkDotNet reports
- use the results before starting deeper optimization work

## What it measures

The benchmark suite currently covers:

- timeline construction from commit data
- timeline MessagePack serialization and deserialization
- timeline normalization asregression check
- semantic trace event aggregation
- file coupling pair generation
- local Git repository reading with and without file changes
- export orchestration without clone, network, or file persistence
- semantic render event translation from timeline events
- render command dispatch into scene handlers
- hive layout computation and animated convergence
- animation system tween and particle processing
- render state assembly from timeline events
- scene frame update and snapshot preparation
- player-driven render pipeline from playback tick to frame submission
- isolated scene snapshot assembly
- edge visibility planning through scene snapshot assembly
- render event buffering and flush into the pipeline
- scene graph mutation and edge-cache rebuilds
- CPU-side GPU buffer contract preparation
- render frame submission preparation before OpenGL upload

Additional player benchmarks are available for playback clock, cursor, seek, stepper, high-level player orchestration, and player factory costs. They are kept in the same benchmark project, but the primary suite tracks issue #17 and the CPU side render pipeline.

The benchmarks intentionally avoid opening OpenTK windows or requiring GPU access. Git reader benchmarks create local synthetic repositories and do not access the network.

## Commands

Run all benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*Benchmarks*"
```

Run Core timeline benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*Core*"
```

Run timeline builder benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*TimelineBuilderBenchmarks*"
```

Run timeline serialization benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*TimelineSerializationBenchmarks*"
```

Run trace event aggregation benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*TraceEventAggregationBenchmarks*"
```

Run file coupling benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*FileCouplingAggregatorBenchmarks*"
```

Run Git reader benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*GitRepositoryReaderBenchmarks*"
```

Run export orchestration benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*RepositoryExporterBenchmarks*"
```

Run the full CPU-side render pipeline suite:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*Rendering*"
```

Run render event translation benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*RenderEventTranslationBenchmarks*"
```

Run scene snapshot assembly benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*SceneSnapshotAssemblyBenchmarks*"
```

Run layout benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*HiveLayoutBenchmarks*"
```

Run player benchmarks:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*Player*"
```

Run one benchmark group with a shorter sanity check:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*RenderStateAssemblyBenchmarks*" --warmupCount 1 --iterationCount 3
```

Run one Core benchmark group with a shorter sanity check:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*TimelineBuilderBenchmarks*" --warmupCount 1 --iterationCount 3
```

Run one player benchmark group with a shorter sanity check:

```bash
dotnet run -c Release --project Benchmarks/ChangeTrace.Benchmarks.csproj -- --filter "*EventCursorBenchmarks*" --warmupCount 1 --iterationCount 3
```

Or through Task:

```bash
task benchmark
```

## Project Structure

The benchmark project is split first by benchmark type, then by measured domain:

- `Micro/` contains small, isolated CPU-path benchmarks
- `Subsystem/` contains module-level benchmarks with a broader surface area
- `Scenario/` contains workflow-style benchmarks such as export orchestration

Shared deterministic setup stays under `Shared/`:

- `Shared/Core` contains shared Core benchmark data
- `Shared/GIt` contains local synthetic repository setup
- `Shared/Rendering` contains shared render benchmark data
- `Shared/Rendering/Gpu` contains shared GPU buffer benchmark data types
- `Shared/Player` contains shared player timeline setup

Current type-to-domain layout:

- `Micro/Core` contains normalization, aggregation, and file-coupling benchmarks
- `Micro/Player` contains clock, cursor, seek, and stepper benchmarks
- `Micro/Rendering` contains render event translation, layout, and GPU buffer preparation benchmarks
- `Micro/Rendering` also contains animation system and scene command dispatch benchmarks
- `Subsystem/Core` contains timeline builder and serialization benchmarks
- `Subsystem/GIt` contains Git reader benchmarks
- `Subsystem/Player` contains player orchestration and player factory benchmarks
- `Subsystem/Rendering` contains render state and scene snapshot assembly benchmarks
- `Subsystem/Rendering` also contains render-event buffering and scene-graph benchmarks
- `Scenario/GIt` contains export orchestration benchmarks
- `Scenario/Rendering` contains frame update and frame submission benchmarks
- `Scenario/Rendering` also contains player-driven playback-to-render benchmarks

The render benchmark suite maps to issue #17:

- dedicated `Benchmarks/ChangeTrace.Benchmarks.csproj` project
- BenchmarkDotNet dependency and memory diagnostics
- `1k`, `10k`, and `100k` synthetic event sizes
- timeline-to-render-command translation benchmark
- hive layout benchmark
- render state and scene snapshot assembly benchmarks
- CPU-side GPU buffer data preparation benchmark
- local run commands outside the normal application build flow

The Core and Git benchmark suite maps to the current export scalability work:

- synthetic commit inputs at `1k`, `10k`, and `100k` sizes
- file-change density checks at `1`, `4`, and `12` files per commit
- timeline construction with commits only, file changes, and full event generation
- MessagePack `.gittrace` serialization and deserialization costs
- semantic aggregation overhead before downstream consumers
- file coupling pair generation for large commits
- LibGit2Sharp read costs with and without file changes
- export orchestration cost without clone, network, or file persistence

## Reports

BenchmarkDotNet writes reports under:

```text
BenchmarkDotNet.Artifacts/results/
```

The most useful files are usually:

- `*-report-github.md` for pull request comments or issue updates
- `*-report.html` for local inspection
- `*-report.csv` for comparisons and spreadsheets

## Notes

Avoid `--job Dry` for performance readings. It forces very short runs and can produce misleading `MinIterationTime` warnings even when benchmarks are configured with a higher minimum iteration time.

The `Failed to set up priority High` message can appear on Linux when the current user cannot raise process priority. It does not mean the benchmark failed.

Real GPU/OpenTK frame benchmarking should be handled separately because results depend heavily on hardware, drivers, VSync, resolution, and windowing environment.

---

ChangeTrace Benchmarks are built for repeatable local performance checks before optimization work.

[Back to top](#changetrace-benchmarks)
