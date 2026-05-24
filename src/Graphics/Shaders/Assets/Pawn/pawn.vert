#version 460 core

// Pawn vertex shader.
// Expands each visible pawn into a world-space quad.

layout(location = 0) in vec2 aQuad;

struct Pawn
{
    vec2 position;
    float radius;
    float alpha;
    vec4 color;
    float glow;
    vec3 padding;
};

layout(std430, binding = 0) readonly buffer VisiblePawns
{
    Pawn pawns[];
};

uniform mat3 uViewProj;

out vec2 vLocal;
out vec4 vColor;
out float vGlow;
out float vRadius;

void main()
{
    Pawn pawn = pawns[gl_InstanceID];

    // Prevent degenerate quads.
    float radius = max(pawn.radius, 0.001);

    vec2 worldPosition = pawn.position +
    aQuad * radius;

    vec3 clip = uViewProj * vec3(worldPosition, 1.0);
    gl_Position = vec4(clip.xy, 0.0, 1.0);

    float alpha = clamp(pawn.alpha, 0.0, 1.0);

    // Local quad coordinate used by the fragment SDF.
    vLocal = aQuad;

    // Final pawn color with per-pawn alpha applied.
    vColor = vec4(pawn.color.rgb, pawn.color.a * alpha);

    // Clamped glow intensity for shader effects.
    vGlow = clamp(pawn.glow, 0.0, 1.0);

    vRadius = radius;
}