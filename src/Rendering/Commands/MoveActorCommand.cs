using ChangeTrace.Core.Models;

namespace ChangeTrace.Rendering.Commands;

/// <summary>
/// Command to move or spawn an actor avatar toward the target node in the rendering graph.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="Timestamp"/> – The simulation time at which this movement occurs.</item>
/// <item><see cref="Actor"/> – The actor (user or system entity) being moved.</item>
/// <item><see cref="TargetNode"/> – The destination node, either file path or branch name.</item>
/// <item><see cref="IsSpawn"/> – If true, an actor is appearing for the first time (spawn); otherwise, it's move.</item>
/// </list>
/// </remarks>
internal sealed record MoveActorCommand(
    double Timestamp,
    ActorName Actor,
    string TargetNode,
    bool IsSpawn,
    string CommitSha = ""
) : RenderCommand(Timestamp);