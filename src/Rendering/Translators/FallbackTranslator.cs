using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Events;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Translators;

/// <summary>
/// A catch-all translator that handles any <see cref="TraceEvent"/> not explicitly handled by other translators.
/// </summary>
/// <remarks>
/// <para>
/// This translator ensures that every actor in simulation has at least a basic movement rendered,
/// even if event type is unrecognized.
/// </para>
/// <list type="bullet">
/// <item>Moves actor toward target node.</item>
/// <item>Does not produce file nodes, edges, or other visual effects.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class FallbackTranslator : IEventTranslator
{
    /// <summary>
    /// Determines whether this translator can handle given event.
    /// </summary>
    /// <param name="_">The <see cref="TraceEvent"/> (ignored).</param>
    /// <returns>Always <c>true</c>, as this is the fallback translator.</returns>
    public bool CanHandle(TraceEvent _) => true;

    /// <summary>
    /// Translates <see cref="TraceEvent"/> into minimal <see cref="RenderCommand"/>.
    /// </summary>
    /// <param name="evt">The event to translate.</param>
    /// <returns>A single <see cref="MoveActorCommand"/> moving the actor to the target node.</returns>
    public IEnumerable<RenderCommand> Translate(TraceEvent evt)
    {
        yield return new MoveActorCommand(
            evt.Timestamp.UnixSeconds,
            evt.Actor,
            evt.Target,
            IsSpawn: false);
    }
}