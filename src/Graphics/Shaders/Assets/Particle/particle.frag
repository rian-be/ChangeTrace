#version 460 core

// Particle fragment shader.
// Renders each point sprite as a soft glowing particle with:
// - circular masking,
// - bright hot core,
// - colored body,
// - outer halo,
// - rim highlight,
// - directional specular highlight,
// - subtle animated flicker,
// - small sparkle variation.
in vec4 vColor;
in float vGlow;
in float vSeed;
in float vSize;

out vec4 fragColor;

// Global elapsed time in seconds.
// Used for flicker and sparkle animation.
uniform float uTime;

// Simple scalar hash.
// Produces stable pseudo-random values in the [0, 1] range.
float Hash(float x)
{
    return fract(
    sin(x * 127.1) *
    43758.5453123);
}

void main()
{
    // Convert point sprite UV from [0, 1] to [-1, 1].
    vec2 uv = gl_PointCoord * 2.0 - 1.0;
    float r2 = dot(uv, uv);

    // Keep the particle circular.
    if (r2 > 1.0)
    {
        discard;
    }

    float r = sqrt(r2);

    // Bright center.
    float core = 1.0 - smoothstep(0.00, 0.22, r);

    // Main particle body.
    float body = 1.0 - smoothstep(0.18, 0.72, r);

    // Wide soft outer glow.
    float halo = 1.0 - smoothstep(0.28, 1.00, r);

    // Thin rim band near the outer body.
    float rim = smoothstep(0.52, 0.78, r) *
        (1.0 - smoothstep(0.78, 1.00, r));

    // Fake directional highlight.
    vec2 lightDir = normalize(vec2(-0.45, 0.75));

    float highlight = pow(
        max(
        dot(normalize(uv + vec2(0.001)), lightDir),
        0.0),
        8.0) *
        (1.0 - smoothstep(0.15, 0.85, r));

    // Soft time-based brightness modulation.
    float flicker = 0.88 + 0.12 *
        sin(
        uTime * 9.0 +
        vSeed * 37.0);

    // Small randomized sparkle near the core.
    float sparkle = pow(
        max(0.0, 1.0 - r),
        18.0) *
        Hash(vSeed + floor(uTime * 18.0));

    vec3 base = vColor.rgb;

    // Hot center tint.
    vec3 hot = mix(base, vec3(1.0), 0.55);

    vec3 color = base * 0.55;

    color += base * body * 0.75;
    color += hot * core * 1.35;
    color += base * halo * vGlow * 0.85;
    color += hot * rim * 0.22;
    color += vec3(1.0) * highlight * 0.35;
    color += vec3(1.0) * sparkle * vGlow * 0.45;

    color *= flicker;

    float alpha = vColor.a *
    (
        body * 0.65 +
        core * 0.55 +
        halo * vGlow * 0.35 +
        rim * 0.18
    );

    // Fade out near the circular edge.
    alpha *= 1.0 - smoothstep(0.88, 1.0, r);
    alpha = clamp(alpha, 0.0, 1.0);

    // Skip almost invisible pixels.
    if (alpha < 0.004)
    {
        discard;
    }

    fragColor = vec4(color, alpha);
}