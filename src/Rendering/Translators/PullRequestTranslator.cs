using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Translators;

/// <summary>
/// Translates pull request events into render commands for visualization pipelines.
/// </summary>
/// <remarks>
/// <para>
/// Handles creation, merging, and closing of pull requests. Produces a badge for PR,
/// and in case of merges, also produces an edge and a particle burst at the target branch.
/// </para>
/// <list type="bullet">
/// <item>Pull request created → Open badge.</item>
/// <item>Pull request merged → Merge badge + edge + particle effect.</item>
/// <item>Pull request closed → Close badge.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class PullRequestTranslator : IEventTranslator
{
    /// <summary>
    /// Event type handled by this translator.
    /// </summary>
    public Type EventType => typeof(PullRequestEvent);

    /// <summary>
    /// Determines whether this translator can handle a given event.
    /// </summary>
    /// <param name="evt">The event to evaluate.</param>
    /// <returns>True if the event is a pull request event; otherwise false.</returns>
    public bool CanHandle(object evt) => evt is PullRequestEvent;

    /// <summary>
    /// Translates <see cref="PullRequestEvent"/> into one or more <see cref="RenderCommand"/> objects.
    /// </summary>
    /// <param name="evt">The pull request event to translate.</param>
    /// <returns>A sequence of render commands representing the pull request badge and optional merge effects.</returns>
    public IEnumerable<RenderCommand> Translate(object evt)
    {
        if (evt is not PullRequestEvent pullRequest)
            return [];

        var timestamp = pullRequest.Timestamp;
        var branch = pullRequest.Branch;
        var prNum = pullRequest.Number.Value;
        var action = pullRequest.Type switch
        {
            PullRequestEventType.PullRequestCreated => PrBadgeAction.Open,
            PullRequestEventType.PullRequestMerged => PrBadgeAction.Merge,
            PullRequestEventType.PullRequestClosed => PrBadgeAction.Close,
            _ => PrBadgeAction.Open
        };

        var commands = new List<RenderCommand>
        {
            new PullRequestBadgeCommand(timestamp, branch, prNum, action)
        };

        if (pullRequest.Type == PullRequestEventType.PullRequestMerged)
        {
          //  commands.Add(new EdgeCommand(timestamp, branch, pullRequest.Target, EdgeKind.PullRequest, Intensity: 1f));
         //   commands.Add(new ParticleBurstCommand(timestamp, pullRequest.Target, ParticleCount: 80, ColorRgb: 0xA5D6A7));
        }

        return commands;
    }
}
