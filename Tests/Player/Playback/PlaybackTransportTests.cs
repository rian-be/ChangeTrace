using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Playback;
using Xunit;

namespace ChangeTrace.Tests.Player.Playback;

/// <summary>Tests playback transport state transitions without rendering or timeline playback.</summary>
public sealed class PlaybackTransportTests
{
    /// <summary>Play moves the transport to Playing and emits a state change.</summary>
    [Fact]
    public void Play_MovesIdleTransportToPlaying()
    {
        using var transport = new PlaybackTransport();
        var states = new List<PlayerState>();
        transport.OnStateChanged += states.Add;

        var result = transport.Play();

        Assert.True(result.IsSuccess);
        Assert.Equal(PlayerState.Playing, transport.State);
        Assert.Equal([PlayerState.Playing], states);
    }

    /// <summary>Pause fails unless the transport is currently playing.</summary>
    [Fact]
    public void Pause_FailsWhenTransportIsNotPlaying()
    {
        using var transport = new PlaybackTransport();

        var result = transport.Pause();

        Assert.True(result.IsFailure);
        Assert.Equal(PlayerState.Idle, transport.State);
    }

    /// <summary>Stop returns the transport to Idle from Playing.</summary>
    [Fact]
    public void Stop_ReturnsPlayingTransportToIdle()
    {
        using var transport = new PlaybackTransport();
        transport.Play();

        var result = transport.Stop();

        Assert.True(result.IsSuccess);
        Assert.Equal(PlayerState.Idle, transport.State);
    }
}
