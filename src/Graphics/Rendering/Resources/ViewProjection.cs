using ChangeTrace.Rendering;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Resources;

/// <summary>
/// SRP: converts a camera snapshot into an OpenTK Matrix3 (world→NDC).
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

        // Use uniform scaling based on width to maintain 1:1 world aspect ratio
        float sx = zoom * 2.0f / viewW;
        float sy = sx * aspect;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        // Translate by -camPos, then rotate, then scale to NDC
        float tx = (-camPos.X * cos + camPos.Y * sin) * sx;
        float ty = (-camPos.X * sin - camPos.Y * cos) * sy;

        var m = Matrix3.Identity;
        m.M11 = sx * cos;   m.M12 = sy * sin;   m.M13 = 0f;
        m.M21 = -sx * sin;  m.M22 = sy * cos;   m.M23 = 0f;
        m.M31 = tx;         m.M32 = ty;         m.M33 = 1f;
        return m;
    }

    public static Matrix3 BuildDefault(int viewportW, int viewportH)
    {
        // Implementacja metody BuildDefault
        return Matrix3.Identity;
    }
}