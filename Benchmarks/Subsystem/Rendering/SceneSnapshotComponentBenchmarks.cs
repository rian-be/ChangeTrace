using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Rendering;
using ChangeTrace.Benchmarks.Shared.Rendering;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States.Avatars;
using ChangeTrace.Rendering.States.Edges;
using ChangeTrace.Rendering.States.Nodes;
using ChangeTrace.Rendering.States.Particles;

namespace ChangeTrace.Benchmarks.Subsystem.Rendering;

/// <summary>
/// Benchmarks individual scene snapshot assembly components.
/// </summary>
/// <remarks>
/// Separates node, avatar, edge, particle, and final immutable snapshot materialization
/// to isolate the main CPU and allocation contributors inside render-state assembly.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Rendering)]
public class SceneSnapshotComponentBenchmarks
{
    private readonly NodeSnapshotAssembler _nodeAssembler = new();
    private readonly AvatarSnapshotAssembler _avatarAssembler = new();
    private readonly EdgeSnapshotAssembler _edgeAssembler = new();
    private readonly ParticleSnapshotAssembler _particleAssembler = new();

    private RenderBenchmarkFixture _fixture = null!;
    private IReadOnlyList<NodeSnapshot> _prebuiltNodes = null!;
    private IReadOnlyList<AvatarSnapshot> _prebuiltAvatars = null!;
    private IReadOnlyList<EdgeSnapshot> _prebuiltEdges = null!;
    private IReadOnlyList<ParticleSnapshot> _prebuiltParticles = null!;

    /// <summary>
    /// Number of synthetic timeline events represented by the generated scene.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates deterministic render benchmark state for the current event count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _fixture = RenderBenchmarkFixture.Create(EventCount);
        _prebuiltNodes = _nodeAssembler.Assemble(_fixture.Scene.Nodes);
        _prebuiltAvatars = _avatarAssembler.Assemble(_fixture.Scene.Avatars, out _);
        _prebuiltEdges = _edgeAssembler.Assemble(_fixture.Scene);
        _prebuiltParticles = _particleAssembler.Assemble(_fixture.Animation);
    }

    /// <summary>
    /// Builds immutable node snapshots from the live scene graph.
    /// </summary>
    [Benchmark]
    public int AssembleNodeSnapshots()
        => _nodeAssembler.Assemble(_fixture.Scene.Nodes).Count;

    /// <summary>
    /// Builds immutable avatar snapshots and counts active avatars.
    /// </summary>
    [Benchmark]
    public int AssembleAvatarSnapshots()
        => _avatarAssembler.Assemble(_fixture.Scene.Avatars, out var activeAvatarCount).Count
            + activeAvatarCount;

    /// <summary>
    /// Builds immutable edge snapshots after hierarchy visibility filtering.
    /// </summary>
    [Benchmark]
    public int AssembleEdgeSnapshots()
        => _edgeAssembler.Assemble(_fixture.Scene).Count;

    /// <summary>
    /// Captures particle snapshots from the animation system.
    /// </summary>
    [Benchmark]
    public int AssembleParticleSnapshots()
        => _particleAssembler.Assemble(_fixture.Animation).Count;

    /// <summary>
    /// Materializes the final scene snapshot from already-built component collections.
    /// </summary>
    [Benchmark]
    public int MaterializeSceneSnapshot()
    {
        var snapshot = new SceneSnapshot(
            _prebuiltNodes,
            _prebuiltAvatars,
            _prebuiltEdges,
            _prebuiltParticles);

        return snapshot.TotalObjects;
    }
}
