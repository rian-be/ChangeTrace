using StbImageSharp;

namespace ChangeTrace.Graphics.Rendering.Text;

/// <summary>
/// Loads bitmap font atlas data or generates the builtin fallback atlas.
/// </summary>
internal static class FontAtlasLoader
{
    /// <summary>
    /// Loads font atlas pixels from the disk or creates the fallback atlas.
    /// </summary>
    internal static (byte[] pixels, int w, int h) LoadOrCreate(
        string path,
        int atlasW,
        int atlasH,
        int glyphW,
        int glyphH)
    {
        if (File.Exists(path))
        {
            try
            {
                StbImage.stbi_set_flip_vertically_on_load(0);

                using Stream stream =
                    File.OpenRead(path);

                ImageResult image =
                    ImageResult.FromStream(
                        stream,
                        ColorComponents.Grey);

                Console.WriteLine(
                    $"[TextRenderer] Atlas loaded: {image.Width}×{image.Height}");

                return (image.Data, image.Width, image.Height);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[TextRenderer] Failed loading atlas: {ex.Message}");
            }
        }

        Console.WriteLine(
            "[TextRenderer] Generating built-in atlas.");

        return GenerateBuiltInAtlas(
            atlasW,
            atlasH,
            glyphW,
            glyphH);
    }

    /// <summary>
    /// Generates fallback ASCII glyph atlas.
    /// </summary>
    private static (byte[] pixels, int w, int h) GenerateBuiltInAtlas(
        int atlasW,
        int atlasH,
        int glyphW,
        int glyphH)
    {
        byte[] pixels =
            new byte[atlasW * atlasH];

        int charsPerRow =
            atlasW / glyphW;

        IReadOnlyDictionary<int, byte[]> glyphs =
            BuiltInGlyphAtlas.Create();

        for (int codeIdx = 0; codeIdx < 96; codeIdx++)
        {
            int col =
                codeIdx % charsPerRow;

            int row =
                codeIdx / charsPerRow;

            int baseX =
                col * glyphW;

            int baseY =
                row * glyphH;

            if (!glyphs.TryGetValue(
                    codeIdx + 32,
                    out byte[]? cols))
            {
                continue;
            }

            for (int cx = 0;
                 cx < 5 && cx < cols.Length;
                 cx++)
            {
                byte bits =
                    cols[cx];

                for (int cy = 0; cy < 9; cy++)
                {
                    if ((bits & (1 << cy)) == 0)
                        continue;

                    int px =
                        baseX + cx + 1;

                    int py =
                        baseY + cy + 2;

                    if (px >= atlasW ||
                        py >= atlasH)
                    {
                        continue;
                    }

                    pixels[py * atlasW + px] = 255;
                }
            }
        }

        return (pixels, atlasW, atlasH);
    }
}