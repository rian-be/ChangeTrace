#version 430 core

// Glyph vertex shader.
// Expands each visible glyph into a textured quad.

layout(location = 0) in vec2 aCorner;

struct GpuGlyph
{
    vec2 Position;
    vec2 Size;
    vec4 Color;
    int GlyphIndex;
    int FlipY;
    int Kind;
    int Pad0;
};

layout(std430, binding = 1) readonly buffer VisibleGlyphBuffer
{
    GpuGlyph glyphs[];
};

uniform mat3 uViewProj;

uniform vec2 uAtlasSize;
uniform vec2 uGlyphSize;
uniform int uCharsPerRow;

out vec2 vUv;
out vec4 vColor;
flat out int vKind;

void main()
{
    GpuGlyph glyph = glyphs[gl_InstanceID];

    int column = glyph.GlyphIndex % uCharsPerRow;
    int row = glyph.GlyphIndex / uCharsPerRow;

    // Size of one glyph cell in atlas UV space.
    vec2 atlasCell = uGlyphSize / max(uAtlasSize, vec2(1.0));

    vec2 uvOrigin = vec2(float(column), float(row)) *atlasCell;
    vec2 uvCorner = aCorner;

    // Optional vertical UV flip for atlas/layout differences.
    if (glyph.FlipY != 0)
    {
        uvCorner.y =
        1.0 - uvCorner.y;
    }

    vUv =uvOrigin + uvCorner * atlasCell;

    vColor = glyph.Color;
    vKind = glyph.Kind;

    // Build world space glyph quad vertex.
    vec2 worldPosition =
        glyph.Position +
        aCorner * glyph.Size;

    vec3 clip = uViewProj * vec3(worldPosition, 1.0);

    gl_Position = vec4(clip.xy, 0.0, 1.0);
}