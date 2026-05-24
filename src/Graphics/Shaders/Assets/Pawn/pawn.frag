#version 460 core

// Pawn fragment shader.
// Renders the pawn SDF, glow, orbit ring and energy tail.

in vec2 vLocal;
in vec4 vColor;
in float vGlow;
in float vRadius;

out vec4 fragColor;

const float Epsilon = 0.0001;

// Signed distance to a circle.
float CircleSdf(vec2 p, float radius)
{
    return length(p) - radius;
}

// Signed distance to a rounded box.
float RoundedBoxSdf(vec2 p, vec2 halfSize, float radius)
{
    vec2 q = abs(p) - halfSize + radius;

    return  length(max(q, vec2(0.0))) +
    min(max(q.x, q.y), 0.0) -
    radius;
}

// Smooth union between two SDF shapes.
float SmoothUnion(float a, float b, float k)
{
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);

    return mix(b, a, h) -
    k * h * (1.0 - h);
}

// Smooth subtraction between two SDF shapes.
float SmoothSubtract(float a, float b, float k)
{
    float h =
    clamp(
    0.5 - 0.5 * (b + a) / k,
    0.0,
    1.0);

    return mix(a, -b, h) +
    k * h * (1.0 - h);
}

// Builds the pawn silhouette SDF.
float PawnSdf(vec2 p)
{
    p.y *= 1.08;

    float head = CircleSdf(p - vec2(0.0, 0.48), 0.25);
    float torso = RoundedBoxSdf(p - vec2(0.0, -0.06), vec2(0.23, 0.43), 0.18);

    // Slight taper: narrower top, wider lower body.
    torso += p.y * 0.10;

    float shoulders = CircleSdf(p - vec2(0.0, 0.10), 0.38);

    // Keep only the upper shoulder volume.
    shoulders = max(shoulders, p.y - 0.23);

    float base = RoundedBoxSdf(
    p - vec2(0.0, -0.58),
    vec2(0.43, 0.14),
    0.12);

    float pawn = SmoothUnion(head, torso, 0.08);
    pawn = SmoothUnion(pawn, shoulders, 0.08);
    pawn = SmoothUnion(pawn, base, 0.10);

    return pawn;
}

// Elliptical orbit ring mask.
float EllipseRing(vec2 p, vec2 scale, float radius, float thickness)
{
    float d = abs(length(p * scale) - radius);

    return 1.0 - smoothstep(
    thickness * 0.35,
    thickness,
    d);
}

// Directional energy streak on the pawn side.
float EnergyTail(vec2 p, float glow)
{
    vec2 q = p - vec2(0.40, 0.25);
    float streak = exp(-abs(q.y + q.x * 0.28) * 12.0);

    float fadeIn = smoothstep(-0.08, 0.28, q.x);
    float fadeOut = 1.0 - smoothstep(0.25, 1.05, q.x);

    return streak * fadeIn * fadeOut *
    (0.35 + glow * 0.65);
}

// Converts an SDF value into soft alpha.
float SoftShapeAlpha(float sdf, float width)
{
    return 1.0 - smoothstep(0.0,
    width,
    sdf);
}

// Simple fake directional lighting.
vec3 ApplyLighting(vec2 p, vec3 baseColor)
{
    vec2 normalHint = normalize(vec2(-p.x, 0.75 - p.y) + vec2(Epsilon));

    vec2 lightDir = normalize(vec2(-0.45, 0.90));
    float lighting = clamp(dot(normalHint, lightDir) * 0.5 + 0.5, 0.0, 1.0);

    return baseColor *
    (0.42 + lighting * 0.58);
}

void main()
{
    vec2 p = vec2(vLocal.x, vLocal.y * 1.08);

    float glow = clamp(vGlow, 0.0, 1.0);

    float sdf = PawnSdf(vLocal);
    float shape = SoftShapeAlpha(sdf, 0.035);

    // Thin silhouette rim.
    float edge = 1.0 - smoothstep(
    0.010,
    0.070,
    abs(sdf));

    // Wide glow around the pawn SDF.
    float outerGlow = 1.0 - smoothstep(
    0.0,
    0.55,
    abs(sdf));

    outerGlow *= 0.25 + glow * 0.55;

    // Bright core in the head.
    float headCore = 1.0 - smoothstep(
    0.0,
    0.22,
    length(p - vec2(0.0, 0.48)));

    // Softer vertical body core.
    float bodyCore = 1.0 - smoothstep(0.0, 0.55,
    length(
    (p - vec2(0.0, -0.12)) *
    vec2(0.85, 1.25)));

    float core = max(headCore, bodyCore * 0.55);

    vec2 ringPosition = p - vec2(0.0, 0.02);

    // Orbit ring around the pawn.
    float ring = EllipseRing(ringPosition, vec2(1.0, 2.8), 0.58, 0.045);

    // Dim the back side of the ring.
    float ringVisibility = mix(0.42, 1.0, smoothstep(-0.18, 0.12, p.y));
    ring *= ringVisibility;

    float tail = EnergyTail(p, glow);

    vec3 baseColor = vColor.rgb;
    vec3 color = ApplyLighting(p, baseColor);

    color += baseColor * core * 1.15;
    color += baseColor * outerGlow * 0.75;
    color += baseColor * edge * 0.55;
    color += baseColor * ring * (1.75 + glow);
    color += baseColor * tail * 1.35;

    // Small white-hot accents.
    color += vec3(1.0) * core * 0.10;
    color += vec3(1.0) * edge * 0.06;

    float alpha = shape * vColor.a;
    alpha = max(alpha, outerGlow * vColor.a * 0.22);
    alpha = max(alpha, ring * vColor.a * 0.90);
    alpha = max(alpha, tail * vColor.a * 0.55);
    alpha = clamp(alpha, 0.0, 1.0);

    if (alpha < 0.004)
    discard;

    fragColor = vec4(color, alpha);
}