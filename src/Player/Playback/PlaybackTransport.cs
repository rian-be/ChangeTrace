using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Results;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Player.Playback;

/// <summary>
///  transport for controlling playback: play, pause, stop.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Keeps track of <see cref="PlayerState"/> and <see cref="PlaybackMode"/>.</item>
/// <item>Exposes <see cref="OnStateChanged"/> and <see cref="OnTick"/> events.</item>
/// <item>Timer-driven tick for advancing playback, default 16ms interval (~60fps).</item>
/// <item>Thread-safe using internal lock.</item>
/// <item>Registered as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class PlaybackTransport : IPlaybackTransport
{
    private readonly Timer _timer;
    private readonly Lock _lock = new();

    private PlayerState _state = PlayerState.Idle;
    private const int DefaultTickMs = 16;
    
    /// <summary>Current state of the player.</summary>
    public PlayerState State { get { lock (_lock) return _state; } }

    /// <summary>Playback mode (once, loop, ping-pong, etc.).</summary>
    public PlaybackMode Mode
    {
        get { lock (_lock) return field; }
        set { lock (_lock) { field = value; } }
    } = PlaybackMode.Once;

    /// <summary>Fired whenever <see cref="State"/> changes.</summary>
    public event Action<PlayerState>? OnStateChanged;

    /// <summary>Fired on each playback tick (~16ms by default).</summary>
    public event Action? OnTick;

    /// <summary>Initializes the transport with optional tick interval.</summary>
    /// <param name="tickMs">Timer interval in milliseconds.</param>
    public PlaybackTransport(int tickMs = DefaultTickMs)
    {
        _timer = new Timer(_ =>
        {
            try { OnTick?.Invoke(); }
            catch { /* swallow */ }
        }, null, Timeout.Infinite, tickMs);
    }

    /// <summary>Starts playback if not already playing.</summary>
    public Result Play()
    {
        lock (_lock)
        {
            if (_state == PlayerState.Playing) return Result.Success();

            _state = PlayerState.Playing;
            OnStateChanged?.Invoke(_state);

            _timer.Change(0, DefaultTickMs);
        }
        return Result.Success();
    }

    /// <summary>Pauses playback if currently playing.</summary>
    public Result Pause()
    {
        lock (_lock)
        {
            if (_state != PlayerState.Playing)
                return Result.Failure($"Cannot pause while {_state}.");

            _state = PlayerState.Paused;
            OnStateChanged?.Invoke(_state);

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        return Result.Success();
    }

    /// <summary>Stops playback and resets state to idle.</summary>
    public Result Stop()
    {
        lock (_lock)
        {
            _state = PlayerState.Idle;
            OnStateChanged?.Invoke(_state);

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        return Result.Success();
    }

    /// <summary>Disposes the internal timer.</summary>
    public void Dispose() => _timer.Dispose();
}