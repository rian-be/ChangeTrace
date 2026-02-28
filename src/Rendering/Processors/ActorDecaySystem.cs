using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors;

/// <summary>
/// Updates avatar activity and alpha values based on idle time.
/// </summary>
/// <remarks>
/// Avatars fade out gradually when not active. Activity is normalized between 0 and 1,
/// and alpha is interpolated between minimum and full visibility.
/// </remarks>
/// <param name="scene">Scene graph containing avatars.</param>
/// <param name="idleFadeDelay">
/// Time in virtual seconds before avatar fully fades out. Defaults to 8 seconds.
/// </param>
internal sealed class ActorDecaySystem(ISceneGraph scene, float idleFadeDelay = 8f)
{
    /// <summary>
    /// Updates activity levels and alpha for all avatars based on their idle time.
    /// </summary>
    /// <param name="virtualTime">Current virtual timeline time (seconds).</param>
    internal void Tick(double virtualTime)
    {
        foreach (var avatar in scene.Avatars.Values)
        {
            var idle = virtualTime - avatar.LastSeen;

            avatar.ActivityLevel = Math.Max(0f, 1f - (float)(idle / idleFadeDelay));
            avatar.Alpha = 0.3f + 0.7f * avatar.ActivityLevel;
        }
    }
}