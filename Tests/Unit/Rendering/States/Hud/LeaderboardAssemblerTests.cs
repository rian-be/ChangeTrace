using ChangeTrace.Rendering.States.Hud;
using Xunit;

namespace ChangeTrace.Tests.Rendering.States.Hud;

/// <summary>Tests CPU-side leaderboard state assembly.</summary>
public sealed class LeaderboardAssemblerTests
{
    /// <summary>Assemble orders actors by recorded commit count and caches unchanged results.</summary>
    [Fact]
    public void Assemble_OrdersActorsByCommitCount()
    {
        var assembler = new LeaderboardAssembler();
        assembler.RecordActorEvent("rian");
        assembler.RecordActorEvent("alex");
        assembler.RecordActorEvent("rian");

        var leaderboard = assembler.Assemble();
        var cached = assembler.Assemble();

        Assert.Equal("rian", leaderboard[0].Actor);
        Assert.Equal(2, leaderboard[0].EventCount);
        Assert.Equal("alex", leaderboard[1].Actor);
        Assert.Same(leaderboard, cached);
    }

    /// <summary>Reset clears recorded actors and cached leaderboard state.</summary>
    [Fact]
    public void Reset_ClearsLeaderboard()
    {
        var assembler = new LeaderboardAssembler();
        assembler.RecordActorEvent("rian");

        assembler.Reset();

        Assert.Empty(assembler.Assemble());
    }
}
