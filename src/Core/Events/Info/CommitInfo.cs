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
/// <item>Provides convenience property <see cref="Target"/> returning the SHA as string.</item>
/// </list>
/// </remarks>
internal readonly record struct CommitInfo(CommitSha Sha, CommitEventType Type)
{
    /// <summary>
    /// Gets the target identifier for this commit, currently the commit SHA.
    /// </summary>
    public readonly string? Target => Sha.Value; // Provisional why because I didn't want to think. 
}