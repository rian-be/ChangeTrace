using ChangeTrace.Rendering;
using OpenTK.Mathematics;

namespace ChangeTrace.OpenTK.Renderer;

/// <summary>
/// SRP: converts camera snapshot into an OpenTK Matrix3 (world→NDC).
/// Used as the single uniform pushed to every shader.
/// </summary>
internal static class ViewProjection
{
    /// <summary>
    /// Builds a 2D orthographic + camera transform.
    ///
    ///   world → camera-relative → NDC
    ///
    /// Matrix3 packs a 2D affine transform:
    ///   [ sx·cos  -sx·sin  tx ]
    ///   [ sy·sin   sy·cos  ty ]
    ///   [ 0        0        1 ]
    /// </summary>
    internal static Matrix3 Build(Vec2 camPos, float zoom, float rotation, int viewW, int viewH)
    {
        float aspect = (float)viewW / viewH;
        float sx = zoom * 2f / viewW;
        float sy = zoom * 2f / viewH;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        // Translate by -camPos, then rotate, then scale to NDC
        float tx = (-camPos.X * cos + camPos.Y * sin) * sx;
        float ty = (-camPos.X * sin - camPos.Y * cos) * sy;

        return new Matrix3(
            sx * cos, -sx * sin, tx,
            sy * sin,  sy * cos, ty,
            0f,        0f,       1f
        );
    }
}