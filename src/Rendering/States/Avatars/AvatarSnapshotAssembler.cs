using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.States.Avatars;

/// <summary>
/// Builds immutable avatar snapshot collections for rendering.
/// </summary>
internal sealed class AvatarSnapshotAssembler
{
    /// <summary>
    /// Converts live avatar state into render snapshots and counts active avatars.
    /// </summary>
    public List<AvatarSnapshot> Assemble(
        IReadOnlyDictionary<ActorName, ActorAvatar> avatars,
        out int activeAvatarCount)
    {
        var snapshots =
            new List<AvatarSnapshot>(
                avatars.Count);

        activeAvatarCount = 0;

        foreach (var avatar in avatars.Values)
        {
            if (avatar.ActivityLevel > 0.1f)
                activeAvatarCount++;

            snapshots.Add(
                new AvatarSnapshot(
                    avatar.Actor.Value,
                    avatar.Position,
                    avatar.Color,
                    avatar.Alpha,
                    avatar.ActivityLevel,
                    avatar.TargetNodeId));
        }

        return snapshots;
    }
}