using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Events.Info;

/// <summary>
/// Represents metadata about commit within trace event.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Holds the commit SHA via <see cref="Sha"/>.</item>
/// <item>Indicates the type of commit event using <see cref="Type"/>.</item>
/// </list>
/// </remarks>
internal readonly record struct CommitInfo(
    CommitSha Sha,
    FileChangeKind Type
);