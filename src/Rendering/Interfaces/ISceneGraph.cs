using System.Numerics;
using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Scene.Relations;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Defines mutable scene graph abstraction used by rendering layer.
/// </summary>
/// <remarks>
/// Scene graph maintains collections of <see cref="SceneNode"/>, 
/// <see cref="SceneEdge"/>, and <see cref="ActorAvatar"/> objects.
/// <para>
/// Implementations are responsible for lifecycle management, lookup,
/// and temporal updates (e.g., edge decay).
/// </para>
/// </remarks>
internal interface ISceneGraph
{
    /// <summary>
    /// Gets all nodes indexed by their unique identifier.
    /// </summary>
    IReadOnlyDictionary<string, SceneNode> Nodes { get; }

    /// <summary>
    /// Gets all avatars indexed by <see cref="ActorName"/>.
    /// </summary>
    IReadOnlyDictionary<ActorName, ActorAvatar> Avatars { get; }

    /// <summary>
    /// Gets collections of active edges.
    /// </summary>
    IReadOnlyList<SceneEdge> Edges { get; }

    /// <summary>
    /// Gets an existing node or creates a new one if it does not exist.
    /// </summary>
    /// <param name="id">Unique node identifier.</param>
    /// <param name="kind">Type of node.</param>
    /// <param name="position">Initial position if a node is created.</param>
    /// <param name="color">Initial color if a node is created.</param>
    /// <returns>Existing or newly created <see cref="SceneNode"/>.</returns>
    SceneNode GetOrAddNode(string id, NodeKind kind, Vec2 position, Vector4? color = null);

    /// <summary>
    /// Finds node by identifier.
    /// </summary>
    /// <param name="id">Node identifier.</param>
    /// <returns>Matching <see cref="SceneNode"/> or <c>null</c> if not found.</returns>
    SceneNode? FindNode(string id);

    /// <summary>
    /// Removes node by identifier.
    /// </summary>
    /// <param name="id">Node identifier.</param>
    void RemoveNode(string id);

    /// <summary>
    /// Gets an existing avatar or creates a new one if missing.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <param name="spawnPos">Initial spawn position if created.</param>
    /// <param name="color">Avatar color.</param>
    /// <returns>Existing or newly created <see cref="ActorAvatar"/>.</returns>
    ActorAvatar GetOrAddAvatar(ActorName actor, Vec2 spawnPos, Vector4 color);

    /// <summary>
    /// Finds avatar by actor.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <returns>Matching <see cref="ActorAvatar"/> or <c>null</c> if not found.</returns>
    ActorAvatar? FindAvatar(ActorName actor);

    /// <summary>
    /// Removes avatar by actor name.
    /// </summary>
    void RemoveAvatar(ActorName actor);
    void ClearAvatars();

    /// <summary>
    /// Adds a new edge between two nodes.
    /// </summary>
    /// <param name="fromId">Source node identifier.</param>
    /// <param name="toId">Target node identifier.</param>
    /// <param name="kind">Edge type.</param>
    /// <param name="virtualTime">Virtual time of creation.</param>
    void AddEdge(string fromId, string toId, EdgeKind kind, double virtualTime);

    void AddBundledEdge(string fromId, IEnumerable<string> toIds, EdgeKind kind, double virtualTime);
    
    /// <summary>
    /// Updates edges based on current virtual time.
    /// </summary>
    /// <param name="virtualTime">Current virtual time.</param>
    /// <param name="decayRate">Alpha decay rate per time unit.</param>
    void TickEdges(double virtualTime, float decayRate);

    /// <summary>
    /// Returns all nodes of specified <see cref="NodeKind"/>.
    /// </summary>
    /// <param name="kind">Node type filter.</param>
    /// <returns>Enumerable of matching nodes.</returns>
    IEnumerable<SceneNode> NodesOfKind(NodeKind kind);

    /// <summary>
    /// Clears entire scene graph.
    /// </summary>
    void Clear();
}