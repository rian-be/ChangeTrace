using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Events.Info;

/// <summary>
/// Represents metadata about branch within trace event.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Holds the branch name via <see cref="Name"/>.</item>
/// <item>Indicates the type of branch event using <see cref="Type"/>.</item>
/// </list>
/// </remarks>
internal readonly record struct BranchInfo(BranchName Name, BranchEventType Type)
{
    /// <summary>
    /// Gets the branch name associated with this event.
    /// </summary>
    public readonly BranchName Name = Name;

    /// <summary>
    /// Gets the type of branch event.
    /// </summary>
    public readonly BranchEventType Type = Type;
}