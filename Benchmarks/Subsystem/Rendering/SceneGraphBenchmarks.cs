using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Rendering;
using ChangeTrace.Benchmarks.Shared.Rendering;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Benchmarks.Subsystem.Rendering;

/// <summary>
/// Benchmarks scene graph mutation and edge-cache rebuild costs.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Rendering)]
public class SceneGraphBenchmarks
{
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    [Benchmark(Baseline = true)]
    public int BuildHierarchyAndReadEdges()
    {
        var scene = new SceneGraph();
        scene.GetOrCreateRoot();

        for (var i = 0; i < EventCount; i++)
        {
            var branchIndex = i % 64;
            var filePath = $"src/module-{branchIndex}/feature-{i % 32}/file-{i}.cs";
            var node = scene.GetOrAddNode(filePath, NodeKind.File, new Vec2(i % 256, i % 128));
            node.ParentId = $"src/module-{branchIndex}/feature-{i % 32}";
        }

        return scene.Edges.Count + scene.Nodes.Count;
    }

    [Benchmark]
    public int AddBundledEdgesTickAndReadCache()
    {
        var scene = BenchmarkSceneFactory.Create(EventCount);
        var fileNodes = scene.Nodes.Values
            .Where(node => node.Kind == NodeKind.File)
            .Select(node => node.Id)
            .Take(Math.Clamp(EventCount / 4, 256, 4096))
            .ToArray();

        for (var i = 0; i < fileNodes.Length; i += 4)
        {
            var chunk = fileNodes.Skip(i).Take(4).ToArray();
            if (chunk.Length < 2)
                break;

            scene.AddBundledEdge(
                fromId: chunk[0],
                toIds: chunk.Skip(1).ToArray(),
                kind: EdgeKind.Commit,
                virtualTime: i);
        }

        scene.TickEdges(1f / 60f, decayRate: 1f);
        return scene.Edges.Count;
    }
}
