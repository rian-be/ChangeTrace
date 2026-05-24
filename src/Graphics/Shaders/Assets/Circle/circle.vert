#version 430 core

// Instanced circle-node vertex shader.
// Expands one unit quad into a world-space circle billboard
// using node data from the visible-node SSBO.
layout(location = 0) in vec2 aQuad;

// GPU side node data.
// Must match CPU side SSBO layout.
struct CircleNode
{
    vec2 position;
    float radius;
    float kind;

    vec4 color;

    float glow;
    float pad0;
    float pad1;
    float pad2;
};

// Visible nodes prepared by the GPU/CPU culling stage.
layout(std430, binding = 1) readonly buffer VisibleNodes
{
    CircleNode visible[];
};

// 2D view projection matrix.
// Converts world coordinates into clip space.
uniform mat3 uViewProj;

// Local quad position in range around the node center.
// Used by the fragment shader for circular masking.
out vec2 vLocal;

// Node tint color.
out vec4 vColor;

// Node glow intensity.
out float vGlow;

// Node type.
// 0 = folder
// 1 = file
flat out float vKind;

void main()
{
    CircleNode node = visible[gl_InstanceID];

    // Avoid degenerate quads.
    float radius = max(node.radius, 0.001);

    // Expand the unit quad around the node center.
    vec2 worldPosition =
    node.position +
    aQuad * radius;

    vec3 clip =
    uViewProj *
    vec3(worldPosition, 1.0);

    gl_Position =
    vec4(clip.xy, 0.0, 1.0);

    vLocal = aQuad;
    vColor = node.color;
    vGlow = node.glow;
    vKind = node.kind;
}