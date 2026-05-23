using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Events.Info;

/// <summary>
/// Represents metadata about pull request within trace event.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Holds the pull request number via <see cref="Number"/>.</item>
/// <item>Indicates the type of pull request event using <see cref="Type"/>.</item>
/// </list>
/// </remarks>
internal readonly record struct PullRequestInfo(PullRequestNumber Number, PullRequestEventType Type)
{
    /// <summary>
    /// Gets the number of pull request associated with this event.
    /// </summary>
    public readonly PullRequestNumber Number = Number;

    /// <summary>
    /// Gets the type of pull request event.
    /// </summary>
    public readonly PullRequestEventType Type = Type;
}