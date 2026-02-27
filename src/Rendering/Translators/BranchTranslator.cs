using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Translators;

/// <summary>
/// Translates branch related <see cref="TraceEvent"/> objects into <see cref="RenderCommand"/>s.
/// </summary>
/// <remarks>
/// <para>
/// Handles branch creation, deletion, and merge events, generating appropriate visual commands:
/// </para>
/// <list type="bullet">
/// <item>Branch creation → appear label + move actor</item>
/// <item>Branch deletion → disappear label</item>
/// <item>Merge → edge, particle burst, move actor</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class BranchTranslator : IEventTranslator
{
    /// <summary>
    /// Determines whether translator can handle given event.
    /// </summary>
    /// <param name="evt">The <see cref="TraceEvent"/> to check.</param>
    /// <returns><c>true</c> if the event has a branch type; otherwise <c>false</c>.</returns>
    public bool CanHandle(TraceEvent evt) => evt.BranchType.HasValue;

    /// <summary>
    /// Translates branch related <see cref="TraceEvent"/> into one or more <see cref="RenderCommand"/>s.
    /// </summary>
    /// <param name="evt">The event to translate.</param>
    /// <returns>A sequence of <see cref="RenderCommand"/> objects representing the visual effects for the event.</returns>
    public IEnumerable<RenderCommand> Translate(TraceEvent evt)
    {
        double timestamp = evt.Timestamp.UnixSeconds;
        var branch = evt.BranchName?.Value ?? evt.Target;

        return evt.BranchType switch
        {
            BranchEventType.BranchCreated => TranslateBranchCreated(timestamp, evt.Actor, branch),
            BranchEventType.BranchDeleted => TranslateBranchDeleted(timestamp, branch),
            BranchEventType.Merge => TranslateMerge(timestamp, evt.Actor, branch, evt.Target),
            _ => []
        };
    }
    
    /// <summary>
    /// Generates render commands for branch creation event.
    /// </summary>
    /// <param name="timestamp">Event timestamp in seconds.</param>
    /// <param name="actor">The actor who created the branch.</param>
    /// <param name="branch">Branch name.</param>
    /// <returns>Sequence of <see cref="RenderCommand"/>s for branch creation.</returns>
    private static IEnumerable<RenderCommand> TranslateBranchCreated(double timestamp, ActorName actor, string branch)
    {
        yield return new BranchLabelCommand(timestamp, branch, BranchLabelAction.Appear);
        yield return new MoveActorCommand(timestamp, actor, branch, IsSpawn: false);
    }
    
    /// <summary>
    /// Generates render commands for branch deletion event.
    /// </summary>
    /// <param name="timestamp">Event timestamp in seconds.</param>
    /// <param name="branch">Branch name.</param>
    /// <returns>Sequence of <see cref="RenderCommand"/>s for branch deletion.</returns>
    private static IEnumerable<RenderCommand> TranslateBranchDeleted(double timestamp, string branch)
    {
        yield return new BranchLabelCommand(timestamp, branch, BranchLabelAction.Disappear);
    }

    /// <summary>
    /// Generates render commands for merge event between branches.
    /// </summary>
    /// <param name="timestamp">Event timestamp in seconds.</param>
    /// <param name="actor">The actor performing the merge.</param>
    /// <param name="source">Source branch name.</param>
    /// <param name="target">Target branch name.</param>
    /// <returns>Sequence of <see cref="RenderCommand"/>s for branch merge visualization.</returns>
    private static IEnumerable<RenderCommand> TranslateMerge(double timestamp, ActorName actor, string source, string target)
    {
        yield return new EdgeCommand(timestamp, source, target, EdgeKind.Merge);
        yield return new ParticleBurstCommand(timestamp, target, ParticleCount: 40, ColorRgb: 0x4FC3F7);
        yield return new MoveActorCommand(timestamp, actor, target, IsSpawn: false);
    }
}