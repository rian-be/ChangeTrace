using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
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
internal sealed class PullRequestTranslator // : IEventTranslator
{
    /// <summary>
    /// Determines whether this translator can handle a given event.
    /// </summary>
    /// <param name="evt">The event to evaluate.</param>
    /// <returns>True if the event has pull request types; otherwise false.</returns>
    public bool CanHandle(TraceEvent evt) => evt.PullRequest.HasValue;

    /// <summary>
    /// Translates <see cref="TraceEvent"/> representing pull request into one or more <see cref="RenderCommand"/> objects.
    /// </summary>
    /// <param name="evt">The pull request event to translate.</param>
    /// <returns>
    /// A sequence of render commands representing the pull request badge, and optionally an edge and particle burst for merged PRs.
    /// </returns>
    public IEnumerable<RenderCommand> Translate(TraceEvent evt)
    {
        double timestamp = evt.TimeForPlayback;
        var branch = evt.Branch?.Name.Value ?? evt.Target;

        var pullRequest = evt.PullRequest;
        if (!pullRequest.HasValue) yield break;
        var prNum = pullRequest.Value.Number.Value;

        var action = pullRequest.Value.Type switch
        {
            PullRequestEventType.PullRequestCreated => PrBadgeAction.Open,
            PullRequestEventType.PullRequestMerged => PrBadgeAction.Merge,
            PullRequestEventType.PullRequestClosed => PrBadgeAction.Close,
            _ => PrBadgeAction.Open
        };

        yield return new PullRequestBadgeCommand(timestamp, branch, prNum, action);

        if (pullRequest.Value.Type != PullRequestEventType.PullRequestMerged) yield break;

        yield return new EdgeCommand(timestamp, branch, evt.Target, EdgeKind.PullRequest, Intensity: 1f);
        yield return new ParticleBurstCommand(timestamp, evt.Target, ParticleCount: 80, ColorRgb: 0xA5D6A7);
    }
}