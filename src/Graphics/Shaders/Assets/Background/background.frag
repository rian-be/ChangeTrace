#version 330 core

// Fullscreen background shader used for backdrop.
// Renders:
// - layered nebula fog using FBM,
// - infinite world-space grids,
// - sparse procedural stars,
// - soft radial vignette.

in vec2 vPos;
out vec4 fragColor;

// Global elapsed time in seconds.
// Used for nebula drift and star twinkle.
uniform float uTime;

// Camera zoom factor.
// Controls world-space scaling.
uniform float uZoom;

// Camera position in world space.
// Keeps background elements stable while moving.
uniform vec2 uCamPos;

// Deterministic pseudo-random hash.
// Returns a value in the [0, 1] range.
float Hash(vec2 p)
{
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);

    return fract((p3.x + p3.y) * p3.z);
}

// Smooth value noise.
// Interpolates random values between grid cells.
float Noise(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);

    // Cubic smoothing curve.
    f = f * f * (3.0 - 2.0 * f);

    float a = Hash(i);
    float b = Hash(i + vec2(1.0, 0.0));
    float c = Hash(i + vec2(0.0, 1.0));
    float d = Hash(i + vec2(1.0, 1.0));

    return mix(
    mix(a, b, f.x),
    mix(c, d, f.x),
    f.y);
}

// Fractal Brownian Motion.
// Layers multiple octaves of smooth noise
// to create soft nebula-like clouds.
float Fbm(vec2 p)
{
    float value = 0.0;
    float amplitude = 0.5;

    for (int i = 0; i < 3; i++)
    {
        value += Noise(p) * amplitude;

        p *= 2.1;
        amplitude *= 0.5;
    }

    return value;
}

// Infinite anti-aliased world-space grid.
float Grid(vec2 worldPos, float size, float thickness)
{
    vec2 coord = worldPos / size;

    // Distance from nearest grid line.
    vec2 cell = abs(fract(coord - 0.5) - 0.5);

    // Screen space AA width.
    vec2 aa = fwidth(coord);

    vec2 lines = 1.0 - smoothstep(
    aa * thickness,
    aa * thickness * 2.0,
    cell);

    return max(lines.x, lines.y);
}

// Sparse procedural star field.
// Stars are generated per world-space cell
// and softly twinkle over time.
float Stars(vec2 worldPos)
{
    vec2 cell = floor(worldPos / 18.0);
    vec2 local = fract(worldPos / 18.0) - 0.5;

    float seed = Hash(cell);
    
    float mask = step(0.985, seed);

    float dist = length(local);
    float shape = 1.0 - smoothstep(0.02, 0.18, dist);
    float twinkle = 0.75 + 0.25 * sin(uTime * 1.4 + seed * 64.0);

    return mask * shape * twinkle;
}

void main()
{
    vec2 uv = vPos;
    float radius = length(uv);
    
    vec2 worldPos = (uv / max(uZoom, 0.001)) * 1000.0 + uCamPos;
    vec3 baseColor = mix(
        vec3(0.018, 0.024, 0.055),
        vec3(0.004, 0.006, 0.014),
        smoothstep(0.0, 1.35, radius));

    // Animated FBM nebula layer.
    float nebula = Fbm(
        uv * 1.6 +
        uCamPos * 0.00008 +
        uTime * 0.015);

    vec3 color = baseColor;

    color +=
    vec3(0.08, 0.035, 0.16) *
    nebula *
    0.25;
    
    float fineGrid = Grid(worldPos, 400.0, 1.0);
    float majorGrid = Grid(worldPos, 2000.0, 1.35);

    color += vec3(0.20, 0.32, 0.85) * fineGrid * 0.012;
    color += vec3(0.30, 0.48, 1.00) * majorGrid * 0.025;
    
    float stars = Stars(worldPos);

    color += vec3(0.85, 0.92, 1.0) * stars * 0.55;
    
    float vignette = 1.0 - smoothstep(0.65, 1.45, radius) * 0.55;
    color *= vignette;
    fragColor = vec4(color, 1.0);
}