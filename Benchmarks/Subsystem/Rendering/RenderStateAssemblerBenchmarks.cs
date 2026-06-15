using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Rendering;
using ChangeTrace.Benchmarks.Shared.Rendering;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Benchmarks.Subsystem.Rendering;

/// <summary>
/// Benchmarks detailed render-state assembly scenarios.
/// </summary>
/// <remarks>
/// Splits the broad render-state benchmark into variants that isolate interaction and HUD
/// overhead: plain assembly, hovered node resolution, hovered pod overlay data, and
/// leaderboard-enriched HUD assembly.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Rendering)]
public class RenderStateAssemblerBenchmarks
{
    private RenderBenchmarkFixture _fixture = null!;
    private RenderStateAssembler _assembler = null!;
    private SceneNode _hoveredNode = null!;
    private HoveredPodHud _hoveredPod = null!;

    /// <summary>
    /// Number of synthetic timeline events represented by the generated scene.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates deterministic render-state benchmark state for the current event count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _fixture = RenderBenchmarkFixture.Create(EventCount);
        _assembler = new RenderStateAssembler();
        _hoveredNode = ResolveHoveredNode(_fixture.Scene);
        _hoveredPod = CreateHoveredPod(_hoveredNode);
    }

    /// <summary>
    /// Assembles a render state without interaction overlays.
    /// </summary>
    [Benchmark(Baseline = true)]
    public int AssembleWithoutHover()
    {
        var state = _assembler.Assemble(
            virtualTime: 1.0,
            wallDelta: 1.0 / 60.0,
            _fixture.Scene,
            _fixture.Animation,
            _fixture.Camera,
            _fixture.CameraController,
            _fixture.Diagnostics,
            hoveredNode: null,
            hoveredPod: null,
            LayoutMode.SingleTree);

        return state.Scene.TotalObjects;
    }

    /// <summary>
    /// Assembles a render state while a file node is hovered.
    /// </summary>
    [Benchmark]
    public int AssembleWithHoveredNode()
    {
        var state = _assembler.Assemble(
            virtualTime: 1.0,
            wallDelta: 1.0 / 60.0,
            _fixture.Scene,
            _fixture.Animation,
            _fixture.Camera,
            _fixture.CameraController,
            _fixture.Diagnostics,
            hoveredNode: _hoveredNode,
            hoveredPod: null,
            LayoutMode.SingleTree);

        return state.Scene.TotalObjects + state.Hud.Leaderboard.Count;
    }

    /// <summary>
    /// Assembles a render state with pod overlay interaction data.
    /// </summary>
    [Benchmark]
    public int AssembleWithHoveredPod()
    {
        var state = _assembler.Assemble(
            virtualTime: 1.0,
            wallDelta: 1.0 / 60.0,
            _fixture.Scene,
            _fixture.Animation,
            _fixture.Camera,
            _fixture.CameraController,
            _fixture.Diagnostics,
            hoveredNode: _hoveredNode,
            hoveredPod: _hoveredPod,
            LayoutMode.Forest);

        return state.Scene.TotalObjects + state.Hud.Leaderboard.Count;
    }

    /// <summary>
    /// Records actor activity before assembly to include leaderboard snapshot work.
    /// </summary>
    [Benchmark]
    public int RecordActorsThenAssembleHud()
    {
        SeedLeaderboard();

        var state = _assembler.Assemble(
            virtualTime: 1.0,
            wallDelta: 1.0 / 60.0,
            _fixture.Scene,
            _fixture.Animation,
            _fixture.Camera,
            _fixture.CameraController,
            _fixture.Diagnostics,
            hoveredNode: _hoveredNode,
            hoveredPod: _hoveredPod,
            LayoutMode.SingleTree);

        _assembler.Reset();
        return state.Scene.TotalObjects + state.Hud.Leaderboard.Count;
    }

    private void SeedLeaderboard()
    {
        var recorded = 0;

        foreach (var avatar in _fixture.Scene.Avatars.Values)
        {
            _assembler.RecordActorEvent(avatar.Actor.Value, $"commit-{recorded:x8}");
            recorded++;

            if (recorded >= 64)
                break;
        }
    }

    private static SceneNode ResolveHoveredNode(SceneGraph scene)
        => scene.Nodes.Values.First(node => node.Kind == NodeKind.File);

    private static HoveredPodHud CreateHoveredPod(SceneNode node)
        => new(
            Id: $"pod:{node.Id}",
            Label: node.Label,
            Center: node.Position,
            LabelPosition: node.Position,
            Radius: MathF.Max(node.Radius * 4f, 24f),
            ActivityScore: 0.75f,
            ImportanceScore: 0.5f,
            FileIds: [node.Id]);
}
