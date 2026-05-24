#version 430 core

// Edge vertex shader.
// Expands each visible edge into a quad strip.
// The quad is generated procedurally from gl_VertexID.

struct GpuEdge
{
    vec2 From;
    vec2 To;

    vec4 Color;

    float Alpha;
    float WidthStart;
    float WidthEnd;
    float Kind;

    float FromGlow;
    float ToGlow;

    vec2 Padding;
};

// Compacted visible edge buffer.
// Filled by the edge culling compute shader.
layout(std430, binding = 0) readonly buffer VisibleEdges
{
    GpuEdge edges[];
};

// 2D view-projection matrix.
// Converts world coordinates into clip space.
uniform mat3 uViewProj;

out vec4 vColor;
out float vAlpha;

// Signed side coordinate passed to the fragment shader.
// -1 = one side of the strip
//  1 = opposite side of the strip
out float vSide;

// Two triangles forming one edge quad.
const int Corners[6] = int[6](
    0, 1, 2,
    1, 3, 2
);

void main()
{
    GpuEdge edge = edges[gl_InstanceID];

    int corner = Corners[gl_VertexID];

    // Position along the edge.
    // 0 = From
    // 1 = To
    float t = corner < 2
    ? 0.0
    : 1.0;

    // Side of the expanded strip.
    float side = corner == 0 || corner == 2
    ? -1.0
    : 1.0;

    vec2 delta = edge.To - edge.From;

    // Avoid division by zero for very short edges.
    float lengthSq = max(dot(delta, delta), 0.000001);

    vec2 direction = delta * inversesqrt(lengthSq);

    // Perpendicular direction used to expand edge width.
    vec2 normal = vec2(-direction.y, direction.x);

    // Interpolated edge width.
    float width = max(
        mix(edge.WidthStart, edge.WidthEnd, t),
        0.001);

    // Build the final world-space quad vertex.
    vec2 worldPosition =
        mix(edge.From, edge.To, t) +
        normal * width * side * 0.5;

    vec3 clip = uViewProj * vec3(worldPosition, 1.0);

    gl_Position = vec4(clip.xy, 0.0, 1.0);

    // Activity is driven by the stronger endpoint glow.
    float activity = max(edge.FromGlow, edge.ToGlow);
    vColor = edge.Color;

    // Dim inactive edges, keep active edges brighter.
    vAlpha = edge.Alpha *
        mix(
        0.35,
        1.0,
        clamp(activity, 0.0, 1.0));

    vSide = side;
}