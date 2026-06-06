using BenchmarkDotNet.Attributes;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Translators;

namespace ChangeTrace.Benchmarks.Rendering;

/// <summary>
/// Benchmarks translation from semantic timeline events into render commands.
/// </summary>
/// <remarks>
/// Measures CPU-side render event translation before commands are dispatched to the scene graph.
/// This covers the timeline-to-render-command part of the render pipeline without OpenGL work.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class RenderEventTranslationBenchmarks
{
    private TranslationPipeline _pipeline = null!;
    private CommitBundleEvent[] _events = null!;

    /// <summary>
    /// Number of synthetic semantic timeline events to translate.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates deterministic semantic event input and the default render translation pipeline.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _pipeline = TranslationPipeline.Default();
        _events = RenderBenchmarkFixture.CreateCommitBundles(EventCount);
    }

    /// <summary>
    /// Translates all semantic events into render commands and returns total command count.
    /// </summary>
    [Benchmark]
    public int TranslateCommitBundles()
    {
        var commandCount = 0;

        foreach (var evt in _events)
        {
            IReadOnlyList<RenderCommand> commands =
                _pipeline.Translate(evt);

            commandCount += commands.Count;
        }

        return commandCount;
    }
}
