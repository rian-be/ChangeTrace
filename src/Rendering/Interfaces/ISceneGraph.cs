using System.Numerics;
using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Scene.Relations;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Mutable scene graph used by the rendering layer.
/// </summary>
internal interface ISceneGraph
{
    /// <summary>
    /// Scene nodes indexed by identifier.
    /// </summary>
    IReadOnlyDictionary<string, SceneNode> Nodes { get; }

    /// <summary>
    /// Actor avatars indexed by actor name.
    /// </summary>
    IReadOnlyDictionary<ActorName, ActorAvatar> Avatars { get; }

    /// <summary>
    /// Active scene edges.
    /// </summary>
    IReadOnlyList<SceneEdge> Edges { get; }

    /// <summary>
    /// Gets an existing node or creates a new one.
    /// </summary>
    SceneNode GetOrAddNode(
        string id,
        NodeKind kind,
        Vec2 position,
        Vector4? color = null);

    /// <summary>
    /// Finds node by identifier.
    /// </summary>
    SceneNode? FindNode(string id);

    /// <summary>
    /// Removes node by identifier.
    /// </summary>
    void RemoveNode(string id);

    /// <summary>
    /// Gets an existing avatar or creates a new one.
    /// </summary>
    ActorAvatar GetOrAddAvatar(
        ActorName actor,
        Vec2 spawnPos,
        Vector4 color);

    /// <summary>
    /// Finds avatar by actor name.
    /// </summary>
    ActorAvatar? FindAvatar(ActorName actor);

    /// <summary>
    /// Removes avatar by actor name.
    /// </summary>
    void RemoveAvatar(ActorName actor);

    /// <summary>
    /// Removes all avatars.
    /// </summary>
    void ClearAvatars();

    /// <summary>
    /// Adds edge between two nodes.
    /// </summary>
    void AddEdge(
        string fromId,
        string toId,
        EdgeKind kind,
        double virtualTime);

    /// <summary>
    /// Adds bundled edge from one source to multiple targets.
    /// </summary>
    void AddBundledEdge(
        string fromId,
        IEnumerable<string> toIds,
        EdgeKind kind,
        double virtualTime);

    /// <summary>
    /// Updates edge decay and lifetime state.
    /// </summary>
    void TickEdges(
        double virtualTime,
        float decayRate);

    /// <summary>
    /// Returns nodes matching the specified kind.
    /// </summary>
    IEnumerable<SceneNode> NodesOfKind(NodeKind kind);

    /// <summary>
    /// Clears the entire scene graph.
    /// </summary>
    void Clear();
}