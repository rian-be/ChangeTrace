using ChangeTrace.Player;
using ChangeTrace.Player.Enums;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ChangeTrace.Graphics.Input.Keyboard;

/// <summary>
/// Handles playback and timeline keyboard controls.
/// </summary>
internal sealed class PlaybackKeyboardController
{
    /// <summary>
    /// Processes playback keyboard input.
    /// </summary>
    public void Handle(KeyboardState keyboard, TimelinePlayer player)
    {
        // Toggle pause/play.
        if (keyboard.IsKeyPressed(Keys.Space))
        {
            TogglePlayback(player);
        }

        // Stop playback or restart depending on the modifier state.
        if (keyboard.IsKeyPressed(Keys.R))
        {
            if (keyboard.IsKeyDown(Keys.LeftShift))
            {
                player.Play();
            }
            else
            {
                player.Stop();
            }
        }

        // Increase playback speed.
        if (keyboard.IsKeyPressed(Keys.Equal) ||
            keyboard.IsKeyPressed(Keys.KeyPadAdd))
        {
            player.TargetSpeed = Math.Min(player.TargetSpeed * 2.0, 100.0);
        }

        // Decrease playback speed.
        if (keyboard.IsKeyPressed(Keys.Minus) ||
            keyboard.IsKeyPressed(Keys.KeyPadSubtract))
        {
            player.TargetSpeed = Math.Max(player.TargetSpeed / 2.0, 0.1);
        }

        // Apply normal playback speed preset.
        if (keyboard.IsKeyPressed(Keys.D1))
        {
            player.ApplyPreset(SpeedPreset.Normal);
        }

        // Apply 2x playback speed preset.
        if (keyboard.IsKeyPressed(Keys.D2))
        {
            player.ApplyPreset(SpeedPreset.Double);
        }

        // Apply fast playback speed preset.
        if (keyboard.IsKeyPressed(Keys.D3))
        {
            player.ApplyPreset(SpeedPreset.Fast);
        }

        // Apply scrub/slow inspection mode.
        if (keyboard.IsKeyPressed(Keys.D0))
        {
            player.ApplyPreset(SpeedPreset.Scrub);
        }

        // Advance one timeline step forward.
        if (keyboard.IsKeyPressed(Keys.Right))
        {
            player.StepForward();
        }

        // Step one timeline frame backward.
        if (keyboard.IsKeyPressed(Keys.Left))
        {
            player.StepBackward();
        }

        // Seek forward by 10% of total duration.
        if (keyboard.IsKeyPressed(Keys.Period))
        {
            player.SeekRelative(player.DurationSeconds * 0.1);
        }

        // Seek backward by 10% of total duration.
        if (keyboard.IsKeyPressed(Keys.Comma))
        {
            player.SeekRelative(-player.DurationSeconds * 0.1);
        }
    }

    /// <summary>
    /// Toggles playback pause/play state.
    /// </summary>
    private static void TogglePlayback(TimelinePlayer player)
    {
        if (player.State == PlayerState.Playing)
        {
            player.Pause();
        }
        else
        {
            player.Play();
        }
    }
}