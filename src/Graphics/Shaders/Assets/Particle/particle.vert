#version 460 core

// Particle vertex shader.
// Renders each visible particle as one point sprite.
// Particle data is read from a compacted SSBO.

struct Particle
{
    vec2 position;
    float size;
    float alpha;
    vec4 color;
};

// Compacted visible particle buffer.
// Filled by the particle culling compute shader.
layout(std430, binding = 0) readonly buffer VisibleParticles
{
    Particle particles[];
};

// 2D view-projection matrix.
// Converts world coordinates into clip space.
uniform mat3 uViewProj;

// Camera zoom factor.
// Used to convert world-space particle size into screen size.
uniform float uZoom;

out vec4 vColor;
out float vGlow;
out float vSeed;
out float vSize;

// Deterministic pseudo-random hash.
// Used to give each particle a stable variation value.
float Hash(vec2 p)
{
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

void main()
{
    Particle particle = particles[gl_InstanceID];
    vec3 clip = uViewProj * vec3(particle.position, 1.0);

    // Convert particle size into screen space point size.
    float screenSize = max(particle.size * uZoom, 1.0);

    gl_Position = vec4(clip.xy, 0.0, 1.0);
    gl_PointSize = screenSize;

    float alpha = clamp(particle.alpha, 0.0, 1.0);
    vColor = vec4(particle.color.rgb, particle.color.a * alpha);

    // Higher alpha particles receive stronger glow.
    vGlow =
    smoothstep(0.05, 1.0, alpha);

    // Stable per particle random seed.
    vSeed = Hash(particle.position + vec2(particle.size, particle.alpha));
    vSize = screenSize;
}