using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Handlers;
using ChangeTrace.Player.Interfaces;

namespace ChangeTrace.Player.Factory;

/// <summary>
/// Factory for creating <see cref="IBoundaryHandler"/> instances based on playback mode.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Returns singleton instance per request (caller may wrap in DI if needed).</item>
/// <item>Maps <see cref="PlaybackMode.Once"/> → <see cref="OnceBoundaryHandler"/>.</item>
/// <item>Maps <see cref="PlaybackMode.Loop"/> → <see cref="LoopBoundaryHandler"/>.</item>
/// <item>Maps <see cref="PlaybackMode.PingPong"/> → <see cref="PingPongBoundaryHandler"/>.</item>
/// </list>
/// </remarks>
internal static class BoundaryHandlerFactory
{
    /// <summary>
    /// Creates boundary handler for the given <paramref name="mode"/>.
    /// </summary>
    /// <param name="mode">Playback mode to handle.</param>
    /// <returns>Corresponding <see cref="IBoundaryHandler"/> implementation.</returns>
    internal static IBoundaryHandler Create(PlaybackMode mode) => mode switch
    {
        PlaybackMode.Once     => new OnceBoundaryHandler(),
        PlaybackMode.Loop     => new LoopBoundaryHandler(),
        PlaybackMode.PingPong => new PingPongBoundaryHandler(),
        _                     => throw new ArgumentOutOfRangeException(nameof(mode))
    };
}