using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Events;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Translators;

/// <summary>
/// Translates commit related <see cref="TraceEvent"/> objects into <see cref="RenderCommand"/>s.
/// </summary>
/// <remarks>
/// <para>
/// Handles commit events by generating visual effects for files and actor movement.
/// </para>
/// <list type="bullet">
/// <item>Pulses the file node.</item>
/// <item>Moves the actor toward the file node.</item>
/// <item>Draws an edge from actor to file node representing the commit.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class CommitTranslator : IEventTranslator
{
    /// <summary>
    /// Determines whether this translator can handle given event.
    /// </summary>
    /// <param name="evt">The <see cref="TraceEvent"/> to check.</param>
    /// <returns><c>true</c> if the event has a commit type; otherwise <c>false</c>.</returns>
    public bool CanHandle(TraceEvent evt) => evt.CommitType.HasValue;

    /// <summary>
    /// Translates commit related <see cref="TraceEvent"/> into one or more <see cref="RenderCommand"/>s.
    /// </summary>
    /// <param name="evt">The event to translate.</param>
    /// <returns>A sequence of <see cref="RenderCommand"/> objects representing the visual effects for the commit.</returns>
    public IEnumerable<RenderCommand> Translate(TraceEvent evt)
    {
        double timestamp = evt.Timestamp.UnixSeconds;
        var filePath = evt.FilePath?.Value ?? evt.Target;
        var actor = evt.Actor.Value;
        
        yield return new FileNodeCommand(timestamp, filePath, FileNodeAction.Pulse);
        yield return new MoveActorCommand(timestamp, evt.Actor, filePath, IsSpawn: false);
        yield return new EdgeCommand(timestamp, actor, filePath, EdgeKind.Commit);
    }
}