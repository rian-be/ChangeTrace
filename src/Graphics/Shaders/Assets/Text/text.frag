#version 430 core

// Glyph fragment shader.
// Samples the font atlas alpha channel
// and applies per glyph tint/color.

uniform sampler2D uAtlas;

in vec2 vUv;
in vec4 vColor;

// Glyph type/category.
// Currently unused in the fragment stage,
// but available for future styling variations.
flat in int vKind;

out vec4 fragColor;

void main()
{
    // Sample monochrome glyph alpha from the atlas.
    float glyphAlpha = texture(uAtlas, vUv).r;

    // Skip nearly transparent texels.
    if (glyphAlpha <= 0.01)
    discard;

    // Final glyph alpha after vertex tint modulation.
    float alpha = vColor.a * glyphAlpha;
    fragColor = vec4(vColor.rgb, alpha);
}