using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Events.Info;

/// <summary>
/// Represents the core information of a trace event.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains the event timestamp, actor, and primary target.</item>
/// <item>Serves as the minimal immutable payload for a <see cref="TraceEvent"/>.</item>
/// <item>Provides helper method <see cref="WithTimestamp"/> to create a copy with an updated timestamp.</item>
/// </list>
/// </remarks>
internal readonly record struct TraceEventCore(Timestamp Timestamp, ActorName Actor,  string Target)
{
    /// <summary>
    /// Returns a copy of this <see cref="TraceEventCore"/> with a new timestamp.
    /// </summary>
    /// <param name="ts">The new timestamp.</param>
    /// <returns>A new <see cref="TraceEventCore"/> instance with <see cref="Timestamp"/> updated.</returns>
    public TraceEventCore WithTimestamp(Timestamp ts) => this with { Timestamp = ts };
}