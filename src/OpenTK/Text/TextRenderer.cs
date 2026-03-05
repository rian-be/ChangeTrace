using ChangeTrace.OpenTK.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace ChangeTrace.OpenTK.Text;

/// <summary>
/// Minimal bitmap font renderer.
/// Uses a pre-baked monospace atlas texture (PNG, 16×6 ASCII grid, 8×14px per glyph).
/// </summary>
internal sealed class TextRenderer : IDisposable
{
    private const int   AtlasW      = 128;
    private const int   AtlasH      = 84;
    private const int   GlyphW      = 8;
    private const int   GlyphH      = 14;
    private const int   CharsPerRow = AtlasW / GlyphW;

    private readonly int _atlasTexture;
    private readonly int _vao;
    private readonly GpuBuffer<float> _vbo;

    private const int MaxChars = 512;
    private readonly float[] _verts = new float[MaxChars * 6 * 4];

    internal TextRenderer(string atlasPath)
    {
        _atlasTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _atlasTexture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        var (pixels, w, h) = LoadAtlasPixels(atlasPath);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8,
                      w, h, 0, PixelFormat.Red, PixelType.UnsignedByte, pixels);

        _vao = GL.GenVertexArray();
        _vbo = new GpuBuffer<float>();

        GL.BindVertexArray(_vao);
        _vbo.UploadEmpty(MaxChars * 6 * 4);

        int stride = 4 * sizeof(float);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.BindVertexArray(0);
    }

    internal void DrawString(
        string text,
        Vector2 origin,
        float scale,
        Vector4 color,
        Shaders.ShaderProgram shader,
        Matrix3 viewProj)
    {
        int count = Math.Min(text.Length, MaxChars);
        int ptr   = 0;

        for (int i = 0; i < count; i++)
        {
            int ch  = Math.Clamp(text[i] - 32, 0, 95);
            int col = ch % CharsPerRow;
            int row = ch / CharsPerRow;

            float u0 = col * GlyphW  / (float)AtlasW;
            float u1 = u0  + GlyphW  / (float)AtlasW;
            float v0 = row * GlyphH  / (float)AtlasH;
            float v1 = v0  + GlyphH  / (float)AtlasH;

            float x0 = origin.X + i * GlyphW * scale;
            float x1 = x0 + GlyphW * scale;
            float y0 = origin.Y;
            float y1 = y0 + GlyphH * scale;

            ptr = WriteVert(ptr, x0, y0, u0, v0);
            ptr = WriteVert(ptr, x1, y0, u1, v0);
            ptr = WriteVert(ptr, x0, y1, u0, v1);
            ptr = WriteVert(ptr, x1, y0, u1, v0);
            ptr = WriteVert(ptr, x1, y1, u1, v1);
            ptr = WriteVert(ptr, x0, y1, u0, v1);
        }

        _vbo.Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, nint.Zero, ptr * sizeof(float), _verts);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _atlasTexture);

        shader.Use();
        shader.Set("uViewProj", viewProj);
        shader.Set("uAtlas",    0);
        shader.Set("uColor",    color);

        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, count * 6);
        GL.BindVertexArray(0);
    }

    private int WriteVert(int ptr, float x, float y, float u, float v)
    {
        _verts[ptr++] = x; _verts[ptr++] = y;
        _verts[ptr++] = u; _verts[ptr++] = v;
        return ptr;
    }

    // ------------------------------------------------------------------
    // Atlas loading — StbImageSharp + fallback generator
    // ------------------------------------------------------------------

    private static (byte[] pixels, int w, int h) LoadAtlasPixels(string path)
    {
        // Próba 1: załaduj atlas z pliku PNG przez StbImageSharp
        if (File.Exists(path))
        {
            try
            {
                StbImage.stbi_set_flip_vertically_on_load(0);
                using var stream = File.OpenRead(path);
                var img = ImageResult.FromStream(stream, ColorComponents.Grey);
                Console.WriteLine($"[TextRenderer] Atlas loaded: {img.Width}×{img.Height} from '{path}'");
                return (img.Data, img.Width, img.Height);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TextRenderer] Failed to load atlas '{path}': {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"[TextRenderer] Atlas not found at '{path}', generating built-in atlas.");
        }

        // Próba 2: wygeneruj prosty atlas programowo
        return GenerateBuiltInAtlas();
    }

    /// <summary>
    /// Generuje minimalny atlas 8×14px per glyph, 16 kolumn, 6 wierszy.
    /// Każdy glyph to białe piksele tworzące uproszczone znaki 5×9.
    /// Wystarczy do wyświetlania czytelnych etykiet bez zewnętrznego pliku.
    /// </summary>
    private static (byte[] pixels, int w, int h) GenerateBuiltInAtlas()
    {
        const int w = AtlasW;
        const int h = AtlasH;
        var pixels = new byte[w * h];

        // Definicje glyph 5×9 (kolumny bitów, bit 0 = top)
        // Zakodowane jako 5 bajtów — każdy bajt to kolumna, bity 0-8 = wiersze
        var glyphs = BuiltInGlyphData();

        for (int codeIdx = 0; codeIdx < 96; codeIdx++)
        {
            int col = codeIdx % CharsPerRow;
            int row = codeIdx / CharsPerRow;
            int baseX = col * GlyphW;
            int baseY = row * GlyphH;

            if (!glyphs.TryGetValue(codeIdx + 32, out var cols))
                continue;

            for (int cx = 0; cx < 5 && cx < cols.Length; cx++)
            {
                byte colBits = cols[cx];
                for (int cy = 0; cy < 9; cy++)
                {
                    if ((colBits & (1 << cy)) != 0)
                    {
                        int px = baseX + cx + 1;  // +1 margin
                        int py = baseY + cy + 2;  // +2 top margin
                        if (px < w && py < h)
                            pixels[py * w + px] = 255;
                    }
                }
            }
        }

        Console.WriteLine($"[TextRenderer] Built-in atlas generated ({w}×{h})");
        return (pixels, w, h);
    }

    private static Dictionary<int, byte[]> BuiltInGlyphData()
    {
        // ASCII glyph bitmaps 5×9.
        // Każdy wpis: char code → 5 bajtów (kolumny), każdy bajt = 9 bitów wierszy (bit0=góra)
        return new Dictionary<int, byte[]>
        {
            [' ']  = [0x00, 0x00, 0x00, 0x00, 0x00],
            ['!']  = [0x00, 0x00, 0xBF, 0x00, 0x00],
            ['"']  = [0x00, 0x07, 0x00, 0x07, 0x00],
            ['#']  = [0x14, 0x7F, 0x14, 0x7F, 0x14],
            ['$']  = [0x24, 0x2A, 0x7F, 0x2A, 0x12],
            ['%']  = [0x23, 0x13, 0x08, 0x64, 0x62],
            ['&']  = [0x36, 0x49, 0x55, 0x22, 0x50],
            ['\''] = [0x00, 0x05, 0x03, 0x00, 0x00],
            ['(']  = [0x00, 0x1C, 0x22, 0x41, 0x00],
            [')']  = [0x00, 0x41, 0x22, 0x1C, 0x00],
            ['*']  = [0x14, 0x08, 0x3E, 0x08, 0x14],
            ['+']  = [0x08, 0x08, 0x3E, 0x08, 0x08],
            [',']  = [0x00, 0x50, 0x30, 0x00, 0x00],
            ['-']  = [0x08, 0x08, 0x08, 0x08, 0x08],
            ['.']  = [0x00, 0x60, 0x60, 0x00, 0x00],
            ['/']  = [0x20, 0x10, 0x08, 0x04, 0x02],
            ['0']  = [0x3E, 0x51, 0x49, 0x45, 0x3E],
            ['1']  = [0x00, 0x42, 0x7F, 0x40, 0x00],
            ['2']  = [0x42, 0x61, 0x51, 0x49, 0x46],
            ['3']  = [0x21, 0x41, 0x45, 0x4B, 0x31],
            ['4']  = [0x18, 0x14, 0x12, 0x7F, 0x10],
            ['5']  = [0x27, 0x45, 0x45, 0x45, 0x39],
            ['6']  = [0x3C, 0x4A, 0x49, 0x49, 0x30],
            ['7']  = [0x01, 0x71, 0x09, 0x05, 0x03],
            ['8']  = [0x36, 0x49, 0x49, 0x49, 0x36],
            ['9']  = [0x06, 0x49, 0x49, 0x29, 0x1E],
            [':']  = [0x00, 0x36, 0x36, 0x00, 0x00],
            [';']  = [0x00, 0x56, 0x36, 0x00, 0x00],
            ['<']  = [0x08, 0x14, 0x22, 0x41, 0x00],
            ['=']  = [0x14, 0x14, 0x14, 0x14, 0x14],
            ['>']  = [0x00, 0x41, 0x22, 0x14, 0x08],
            ['?']  = [0x02, 0x01, 0x51, 0x09, 0x06],
            ['@']  = [0x32, 0x49, 0x79, 0x41, 0x3E],
            ['A']  = [0x7E, 0x11, 0x11, 0x11, 0x7E],
            ['B']  = [0x7F, 0x49, 0x49, 0x49, 0x36],
            ['C']  = [0x3E, 0x41, 0x41, 0x41, 0x22],
            ['D']  = [0x7F, 0x41, 0x41, 0x22, 0x1C],
            ['E']  = [0x7F, 0x49, 0x49, 0x49, 0x41],
            ['F']  = [0x7F, 0x09, 0x09, 0x09, 0x01],
            ['G']  = [0x3E, 0x41, 0x49, 0x49, 0x7A],
            ['H']  = [0x7F, 0x08, 0x08, 0x08, 0x7F],
            ['I']  = [0x00, 0x41, 0x7F, 0x41, 0x00],
            ['J']  = [0x20, 0x40, 0x41, 0x3F, 0x01],
            ['K']  = [0x7F, 0x08, 0x14, 0x22, 0x41],
            ['L']  = [0x7F, 0x40, 0x40, 0x40, 0x40],
            ['M']  = [0x7F, 0x02, 0x0C, 0x02, 0x7F],
            ['N']  = [0x7F, 0x04, 0x08, 0x10, 0x7F],
            ['O']  = [0x3E, 0x41, 0x41, 0x41, 0x3E],
            ['P']  = [0x7F, 0x09, 0x09, 0x09, 0x06],
            ['Q']  = [0x3E, 0x41, 0x51, 0x21, 0x5E],
            ['R']  = [0x7F, 0x09, 0x19, 0x29, 0x46],
            ['S']  = [0x46, 0x49, 0x49, 0x49, 0x31],
            ['T']  = [0x01, 0x01, 0x7F, 0x01, 0x01],
            ['U']  = [0x3F, 0x40, 0x40, 0x40, 0x3F],
            ['V']  = [0x1F, 0x20, 0x40, 0x20, 0x1F],
            ['W']  = [0x3F, 0x40, 0x38, 0x40, 0x3F],
            ['X']  = [0x63, 0x14, 0x08, 0x14, 0x63],
            ['Y']  = [0x07, 0x08, 0x70, 0x08, 0x07],
            ['Z']  = [0x61, 0x51, 0x49, 0x45, 0x43],
            ['[']  = [0x00, 0x7F, 0x41, 0x41, 0x00],
            ['\\'] = [0x02, 0x04, 0x08, 0x10, 0x20],
            [']']  = [0x00, 0x41, 0x41, 0x7F, 0x00],
            ['^']  = [0x04, 0x02, 0x01, 0x02, 0x04],
            ['_']  = [0x40, 0x40, 0x40, 0x40, 0x40],
            ['`']  = [0x00, 0x01, 0x02, 0x04, 0x00],
            ['a']  = [0x20, 0x54, 0x54, 0x54, 0x78],
            ['b']  = [0x7F, 0x48, 0x44, 0x44, 0x38],
            ['c']  = [0x38, 0x44, 0x44, 0x44, 0x20],
            ['d']  = [0x38, 0x44, 0x44, 0x48, 0x7F],
            ['e']  = [0x38, 0x54, 0x54, 0x54, 0x18],
            ['f']  = [0x08, 0x7E, 0x09, 0x01, 0x02],
            ['g']  = [0x0C, 0x52, 0x52, 0x52, 0x3E],
            ['h']  = [0x7F, 0x08, 0x04, 0x04, 0x78],
            ['i']  = [0x00, 0x44, 0x7D, 0x40, 0x00],
            ['j']  = [0x20, 0x40, 0x44, 0x3D, 0x00],
            ['k']  = [0x7F, 0x10, 0x28, 0x44, 0x00],
            ['l']  = [0x00, 0x41, 0x7F, 0x40, 0x00],
            ['m']  = [0x7C, 0x04, 0x18, 0x04, 0x78],
            ['n']  = [0x7C, 0x08, 0x04, 0x04, 0x78],
            ['o']  = [0x38, 0x44, 0x44, 0x44, 0x38],
            ['p']  = [0x7C, 0x14, 0x14, 0x14, 0x08],
            ['q']  = [0x08, 0x14, 0x14, 0x18, 0x7C],
            ['r']  = [0x7C, 0x08, 0x04, 0x04, 0x08],
            ['s']  = [0x48, 0x54, 0x54, 0x54, 0x20],
            ['t']  = [0x04, 0x3F, 0x44, 0x40, 0x20],
            ['u']  = [0x3C, 0x40, 0x40, 0x20, 0x7C],
            ['v']  = [0x1C, 0x20, 0x40, 0x20, 0x1C],
            ['w']  = [0x3C, 0x40, 0x30, 0x40, 0x3C],
            ['x']  = [0x44, 0x28, 0x10, 0x28, 0x44],
            ['y']  = [0x0C, 0x50, 0x50, 0x50, 0x3C],
            ['z']  = [0x44, 0x64, 0x54, 0x4C, 0x44],
            ['{']  = [0x00, 0x08, 0x36, 0x41, 0x00],
            ['|']  = [0x00, 0x00, 0x7F, 0x00, 0x00],
            ['}']  = [0x00, 0x41, 0x36, 0x08, 0x00],
            ['~']  = [0x10, 0x08, 0x08, 0x10, 0x08],
        };
    }

    public void Dispose()
    {
        GL.DeleteTexture(_atlasTexture);
        GL.DeleteVertexArray(_vao);
        _vbo.Dispose();
    }
}