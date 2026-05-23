using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Core.Diagnostics;

namespace ChangeTrace.Rendering.Processors;

/// <summary>
/// Updates avatar activity and alpha values based on real-time delta.
/// </summary>
internal sealed class ActorDecaySystem(IDiagnosticsProvider diagnostics)
{
    /// <summary>
    /// Updates activity levels and alpha for all avatars based on real time delta.
    /// Removes fully faded avatars from the scene.
    /// </summary>
    internal void Tick(ISceneGraph? scene, double wallDelta)
    {
        var fDelta = (float)wallDelta;
        if (fDelta <= 0 || scene == null) return;

        var keys = scene.Avatars.Keys.ToList();
        
        foreach (var key in keys)
        {
            var avatar = scene.FindAvatar(key);
            if (avatar == null) continue;
            
            float decayAmount = fDelta / 4.0f;
            avatar.ActivityLevel -= decayAmount;
            
            // Remove avatar when activity drops below the rendering threshold (0.1)
            if (avatar.ActivityLevel <= 0.1f)
            {
                avatar.ActivityLevel = 0f;
                avatar.Alpha = 0f;
                scene.RemoveAvatar(key);
                diagnostics.RecordEvent("Scene.AvatarRemoved", avatar.Actor.Value);
                continue;
            }

            // Existing fade logic
            avatar.Alpha = Math.Clamp(avatar.ActivityLevel * 2.0f, 0f, 1f);

            // DRIFT: Avatars follow their target nodes as they move in the layout
            if (avatar.TargetNodeId == null) continue;
            var targetNode = scene.FindNode(avatar.TargetNodeId);
            if (targetNode == null) continue;
            
            float lerpFactor = 1.0f - MathF.Exp(-64.0f * fDelta);
            avatar.Position = Vec2.Lerp(avatar.Position, targetNode.Position, Math.Min(1.0f, lerpFactor));
        }
        
        diagnostics.RecordMetric("Scene.Decay.Delta", fDelta);
    }
}