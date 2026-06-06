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
internal readonly record struct PullRequestInfo(PullRequestNumber Number, PullRequestEventType Type);
