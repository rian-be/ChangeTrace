namespace ChangeTrace.Core.Events.Semantic;

/// <summary>
/// Represents an actor performing an action on target at specific timestamp.
/// Useful for tracking activity for rendering, analysis, or timeline aggregation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Records the virtual time of the action via <see cref="Timestamp"/>.</item>
/// <item>Records the actor performing the action via <see cref="Actor"/>.</item>
/// <item>Records the target of the action via <see cref="Target"/>.</item>
/// <item>Can be implicitly converted to <see cref="SemanticEvent"/> to integrate with generic semantic pipelines.</item>
/// </list>
/// </remarks>
internal readonly struct ActorActivityEvent(double timestamp, string actor, string target)
{
    /// <summary>Gets the event timestamp (Unix seconds).</summary>
    public readonly double Timestamp = timestamp;

    /// <summary>Gets the actor performing the action.</summary>
    public readonly string Actor = actor;

    /// <summary>Gets the target of the action.</summary>
    public readonly string Target = target;

    /// <summary>Implicitly converts this event to a <see cref="SemanticEvent"/>.</summary>
    /// <param name="e">The actor activity event.</param>
    public static implicit operator SemanticEvent(ActorActivityEvent e)
        => new(e.Timestamp);
}