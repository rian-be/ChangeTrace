using ChangeTrace.Player.Enums;

namespace ChangeTrace.Player.Speed;

/// <summary>
/// Provides numeric multipliers for <see cref="SpeedPreset"/> values.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>QuarterSpeed → 0.25×</item>
/// <item>HalfSpeed → 0.5×</item>
/// <item>Normal → 1×</item>
/// <item>Double → 2×</item>
/// <item>Fast → 5×</item>
/// <item>VeryFast → 20×</item>
/// <item>Scrub → 100×</item>
/// </list>
/// </remarks>
internal static class SpeedPresets
{
    private static readonly IReadOnlyDictionary<SpeedPreset, double> Values =
        new Dictionary<SpeedPreset, double>
        {
            [SpeedPreset.QuarterSpeed]  = 0.25,
            [SpeedPreset.HalfSpeed]     = 0.50,
            [SpeedPreset.Normal]        = 1.00,
            [SpeedPreset.Double]        = 2.00,
            [SpeedPreset.Fast]          = 5.00,
            [SpeedPreset.VeryFast]      = 20.0,
            [SpeedPreset.Scrub]         = 100.0,
        };

    /// <summary>
    /// Attempts to get numeric speed multiplier for a <see cref="SpeedPreset"/>.
    /// </summary>
    /// <param name="preset">Preset enum value.</param>
    /// <param name="speed">Output multiplier if found.</param>
    /// <returns>True if preset exists; false otherwise.</returns>
    internal static bool TryGet(SpeedPreset preset, out double speed)
        => Values.TryGetValue(preset, out speed);
}