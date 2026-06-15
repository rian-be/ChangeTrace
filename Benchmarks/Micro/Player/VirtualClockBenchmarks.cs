using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Shared.Player;

namespace ChangeTrace.Benchmarks.Micro.Player;

/// <summary>
/// Benchmarks virtual clock control operations used by playback orchestration.
/// </summary>
/// <remarks>
/// Measures deterministic clock paths without relying on timer scheduling:
/// snapping position, ramp target changes, and freeze/reset transitions.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Player)]
public class VirtualClockBenchmarks
{
    /// <summary>
    /// Target speed applied when measuring ramp setup.
    /// </summary>
    [Params(2.0, 8.0, 32.0)]
    public double TargetSpeed { get; set; }

    /// <summary>
    /// Snaps virtual position and reads it back.
    /// </summary>
    [Benchmark(Baseline = true)]
    public double SnapPositionAndReadVirtualNow()
    {
        var clock = PlayerBenchmarkFixture.CreateClock();
        clock.SnapPosition(512);
        return clock.VirtualNow;
    }

    /// <summary>
    /// Sets a new speed target and reads back target metadata.
    /// </summary>
    [Benchmark]
    public double SetTargetSpeedAndReadTarget()
    {
        var clock = PlayerBenchmarkFixture.CreateClock();
        clock.Start();
        clock.SetTargetSpeed(TargetSpeed);
        return clock.TargetSpeed;
    }

    /// <summary>
    /// Freezes the clock after speed and position changes, then reads the current speed.
    /// </summary>
    [Benchmark]
    public double FreezeAfterSnapSpeed()
    {
        var clock = PlayerBenchmarkFixture.CreateClock();
        clock.SnapSpeed(TargetSpeed);
        clock.SnapPosition(512);
        clock.Freeze();
        return clock.CurrentSpeed;
    }
}
