using System.Numerics;
using BenchmarkDotNet.Attributes;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Benchmarks.Micro.Rendering;

/// <summary>
/// Benchmarks particle and tween processing inside the renderer animation system.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Rendering)]
public class AnimationSystemBenchmarks
{
    private AnimationSystem _animation = null!;
    private List<ParticleSnapshot> _snapshots = null!;
    private int _tweenCount;
    private int _burstCount;

    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _tweenCount = Math.Clamp(EventCount / 16, 64, 4_096);
        _burstCount = Math.Clamp(EventCount / 1_000, 4, 128);
        _snapshots = new List<ParticleSnapshot>(Math.Min(15_000, Math.Max(32, EventCount / 4)));
        RebuildAnimation();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _snapshots.Clear();
        RebuildAnimation();
    }

    [Benchmark(Baseline = true)]
    public int TickActiveTweensAndParticles()
    {
        _animation.Tick(1f / 60f);
        return _animation.ParticleCount;
    }

    [Benchmark]
    public int SnapshotParticles()
    {
        _animation.SnapshotParticles(_snapshots);
        return _snapshots.Count;
    }

    private void RebuildAnimation()
    {
        _animation = new AnimationSystem();

        for (var i = 0; i < _tweenCount; i++)
        {
            var from = new Vec2(i % 128, i % 64);
            var to = new Vec2((i % 128) + 10, (i % 64) + 5);
            _animation.TweenVec2(from, to, 1.2f, Easing.EaseOutCubic, _ => { }, tag: $"vec-{i}");
            _animation.TweenFloat(1f, 0f, 0.8f, Easing.EaseOutQuad, _ => { }, tag: $"float-{i}");
        }

        for (var i = 0; i < _burstCount; i++)
        {
            _animation.Burst(
                new Vec2(i * 3, i * 2),
                count: 25,
                color: new Vector4(0.1f, 0.7f, 1f, 1f));
        }
    }
}
