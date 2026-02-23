namespace ChangeTrace.Player.Enums;

/// <summary>
/// Predefined speed multipliers for playback control.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="QuarterSpeed"/> – 0.25×, slow motion.</item>
/// <item><see cref="HalfSpeed"/> – 0.5×, half speed.</item>
/// <item><see cref="Normal"/> – 1×, real time.</item>
/// <item><see cref="Double"/> – 2×, double speed.</item>
/// <item><see cref="Fast"/> – 5×, fast forward.</item>
/// <item><see cref="VeryFast"/> – 20×, very fast playback.</item>
/// <item><see cref="Scrub"/> – 100×, instant scrub / near-instant jump.</item>
/// </list>
/// </remarks>
internal enum SpeedPreset
{
    /// <summary>0.25× – slow motion.</summary>
    QuarterSpeed,

    /// <summary>0.5× – half speed.</summary>
    HalfSpeed,

    /// <summary>1× – real time.</summary>
    Normal,

    /// <summary>2× – double speed.</summary>
    Double,

    /// <summary>5× – fast forward.</summary>
    Fast,

    /// <summary>20× – very fast.</summary>
    VeryFast,

    /// <summary>100× – instant scrub / near instant jump.</summary>
    Scrub
}