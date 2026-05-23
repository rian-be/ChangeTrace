// Graph/SceneAvatarRegistry.cs
using System.Numerics;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Rendering.Scene.Graph;

/// <summary>
/// Stores and manages actor avatars.
/// </summary>
internal sealed class SceneAvatarRegistry
{
    /// <summary>
    /// Registered avatars indexed by actor name.
    /// </summary>
    private readonly Dictionary<ActorName, ActorAvatar> _avatars = [];

    /// <summary>
    /// Registered scene avatars.
    /// </summary>
    public IReadOnlyDictionary<ActorName, ActorAvatar> Items => _avatars;

    /// <summary>
    /// Gets existing avatar or creates a new one.
    /// </summary>
    public ActorAvatar GetOrAdd(
        ActorName actor,
        Vec2 spawnPos,
        Vector4 color)
    {
        if (_avatars.TryGetValue(actor, out ActorAvatar? existing))
            return existing;

        var avatar = new ActorAvatar(actor, spawnPos, color);

        _avatars[actor] = avatar;

        return avatar;
    }

    /// <summary>
    /// Finds avatar by actor identifier.
    /// </summary>
    public ActorAvatar? Find(ActorName actor)
    {
        return _avatars.TryGetValue(actor, out ActorAvatar? avatar)
            ? avatar
            : null;
    }

    /// <summary>
    /// Removes avatar by actor identifier.
    /// </summary>
    public void Remove(ActorName actor)
    {
        _avatars.Remove(actor);
    }

    /// <summary>
    /// Removes all avatars.
    /// </summary>
    public void Clear()
    {
        _avatars.Clear();
    }
}