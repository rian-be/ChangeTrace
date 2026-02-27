namespace ChangeTrace.Rendering.Animation;

/// <summary>
/// Delegate representing tween easing function.
/// Maps normalized progress t ∈ [0,1] to eased progress ∈ [0,1].
/// </summary>
/// <param name="t">Normalized progress (0 = start, 1 = end).</param>
/// <returns>Eased progress.</returns>
internal delegate float EasingFn(float t);