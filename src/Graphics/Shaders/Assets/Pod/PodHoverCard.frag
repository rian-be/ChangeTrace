#version 430 core

// Pod hover-card fragment shader.
// Renders:
// - glassmorphism panel,
// - rounded pills/chips,
// - border glow,
// - pointer triangle,
// - layered sci-fi gradients.

in vec2 vUv;
in vec2 vPixel;

uniform vec4 uRectPx;
uniform vec4 uPanelPx;
uniform vec4 uBadgePx;
uniform vec4 uChip0Px;
uniform vec4 uChip1Px;
uniform vec4 uChip2Px;
uniform vec2 uPodCenterPx;

uniform float uRadius;
uniform float uBorderWidth;
uniform float uGlowSize;
uniform float uTime;

out vec4 FragColor;

const vec3 SPACE_BLUE = vec3(0.02, 0.03, 0.08);
const vec3 DARK_VOID = vec3(0.01, 0.01, 0.02);
const vec3 NEBULA = vec3(0.10, 0.05, 0.20);
const vec3 GRID_BLUE = vec3(0.30, 0.40, 1.00);
const vec3 STAR_WHITE = vec3(0.90, 0.95, 1.00);

struct PanelMask
{
    float sdf;
    float rectSdf;
    float shape;
    float rectShape;
    float border;
    float outerGlow;
    vec2 uv;
};

// Rounded rectangle SDF.
float roundedRectSdf(vec2 p, vec2 halfSize, float radius)
{
    vec2 q = abs(p) - halfSize + vec2(radius);

    return length(max(q, 0.0)) +
    min(max(q.x, q.y), 0.0) -
    radius;
}

// Triangle SDF used for the panel pointer.
float triangleSdf(vec2 p, vec2 a, vec2 b, vec2 c)
{
    vec2 e0 = b - a;
    vec2 e1 = c - b;
    vec2 e2 = a - c;

    vec2 v0 = p - a;
    vec2 v1 = p - b;
    vec2 v2 = p - c;

    vec2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
    vec2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
    vec2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);

    float winding = sign(e0.x * e2.y - e0.y * e2.x);

    vec2 d = min(
        min(
            vec2(dot(pq0, pq0), winding * (v0.x * e0.y - v0.y * e0.x)),
            vec2(dot(pq1, pq1), winding * (v1.x * e1.y - v1.y * e1.x))
        ),
        vec2(dot(pq2, pq2), winding * (v2.x * e2.y - v2.y * e2.x))
    );

    return -sqrt(d.x) * sign(d.y);
}

// Converts SDF into a soft border stroke.
float strokeFromSdf(float sdf, float width, float aa)
{
    float outer = 1.0 - smoothstep(0.0, aa, sdf);
    float inner = 1.0 - smoothstep(-width, -width - aa, sdf);

    return clamp(outer - inner, 0.0, 1.0);
}

// Thin horizontal divider line.
float horizontalLine(float y, float center, float thickness)
{
    return 1.0 - smoothstep(
        thickness,
        thickness + 0.005,
        abs(y - center)
    );
}

// Standard alpha-over blending.
vec4 blendOver(vec4 dst, vec4 src)
{
    float outA = src.a + dst.a * (1.0 - src.a);

    if (outA <= 0.0001)
    return vec4(0.0);

    vec3 outRgb = (src.rgb * src.a + dst.rgb * dst.a * (1.0 - src.a)) / outA;

    return vec4(outRgb, outA);
}

// Rectangle center in pixel space.
vec2 rectCenter(vec4 rectPx)
{
    return rectPx.xy + rectPx.zw * 0.5;
}

// Converts pixel position into local rect UV.
vec2 rectUv(vec4 rectPx)
{
    return clamp((vPixel - rectPx.xy) / rectPx.zw, 0.0, 1.0);
}

// Rounded rectangle mask helper.
float rectMask(vec4 rectPx, float radius, float aa)
{
    vec2 center = rectCenter(rectPx);
    vec2 p = vPixel - center;

    float sdf = roundedRectSdf(p, rectPx.zw * 0.5, radius);

    return 1.0 - smoothstep(0.0, aa, sdf);
}

// Creates layered sci-fi glass shading.
vec3 makeGlassColor(vec2 uv, float rectShape)
{
    float top = 1.0 - uv.y;

    float header = 1.0 - smoothstep(0.0, 0.34, uv.y);
    float bottom = smoothstep(0.55, 1.0, uv.y);
    float topHighlight = smoothstep(0.80, 1.0, top);

    float dividerA = horizontalLine(uv.y, 0.455, 0.003) * rectShape;
    float dividerB = horizontalLine(uv.y, 0.745, 0.003) * rectShape;
    float topLine  = horizontalLine(uv.y, 0.050, 0.004) * rectShape;

    vec3 glass = mix(
        DARK_VOID,
        SPACE_BLUE,
        0.56 + top * 0.22
    );

    glass += NEBULA * header * 0.18;
    glass += GRID_BLUE * topHighlight * 0.035;
    glass -= DARK_VOID * bottom * 0.34;

    glass += GRID_BLUE * topLine * 0.055;
    glass += GRID_BLUE * (dividerA + dividerB) * 0.040;

    return glass;
}

// Builds the panel SDF + derived masks.
PanelMask makePanelMask()
{
    vec2 panelCenter = rectCenter(uPanelPx);
    vec2 panelSize = uPanelPx.zw;
    vec2 p = vPixel - panelCenter;

    float rectSdf = roundedRectSdf(
        p,
        panelSize * 0.5,
        uRadius
    );

    // Pointer triangle aligned toward hovered pod.
    float pointerX = clamp(
        uPodCenterPx.x - panelCenter.x,
        -panelSize.x * 0.36,
        panelSize.x * 0.36
    );

    vec2 pointerA = vec2(pointerX - 13.0, panelSize.y * 0.5 - 1.0);
    vec2 pointerB = vec2(pointerX + 13.0, panelSize.y * 0.5 - 1.0);
    vec2 pointerC = vec2(pointerX, panelSize.y * 0.5 + 18.0);

    float pointerSdf = triangleSdf(
        p,
        pointerA,
        pointerB,
        pointerC
    );

    float sdf = min(rectSdf, pointerSdf);
    float aa = 1.20;

    PanelMask mask;

    mask.sdf = sdf;
    mask.rectSdf = rectSdf;
    mask.shape = 1.0 - smoothstep(0.0, aa, sdf);
    mask.rectShape = 1.0 - smoothstep(0.0, aa, rectSdf);
    mask.border = strokeFromSdf(sdf, max(uBorderWidth, 1.25), aa);
    mask.outerGlow = 1.0 - smoothstep(0.0, uGlowSize, max(sdf, 0.0));
    mask.uv = rectUv(uPanelPx);

    return mask;
}

// Final panel shading.
vec4 makePanelColor(PanelMask mask)
{
    float pulse = 0.99 + 0.01 * sin(uTime * 1.2);

    vec3 glass = makeGlassColor(mask.uv, mask.rectShape);

    vec3 borderRgb = mix(
        SPACE_BLUE,
        GRID_BLUE,
        0.32
    );

    float fillAlpha = mask.shape * 0.82;
    float borderAlpha = mask.border * 0.62;
    float glowAlpha = mask.outerGlow * 0.014 * pulse;

    vec3 rgb =
    glass * fillAlpha +
    borderRgb * borderAlpha +
    GRID_BLUE * glowAlpha;

    float alpha = max(
        fillAlpha,
        max(borderAlpha, glowAlpha)
    );

    return vec4(rgb, alpha);
}

// Generic glass pill/chip renderer.
vec4 makePill(vec4 rectPx, vec3 accent, float radius, float alphaMul, float parentMask)
{
    vec2 center = rectCenter(rectPx);
    vec2 p = vPixel - center;

    float sdf = roundedRectSdf(
        p,
        rectPx.zw * 0.5,
        radius
    );

    float aa = 1.15;
    float mask = 1.0 - smoothstep(0.0, aa, sdf);
    float border = strokeFromSdf(sdf, 1.05, aa);

    float glow = 1.0 - smoothstep(
        0.0,
        6.0,
        max(sdf, 0.0)
    );

    vec2 uv = rectUv(rectPx);
    float top = 1.0 - uv.y;

    vec3 fill = mix(
        DARK_VOID,
        SPACE_BLUE,
        0.55 + top * 0.25
    );

    fill += NEBULA * 0.055;
    fill += accent * 0.018;

    vec3 rgb =
    fill * mask +
    accent * border * 0.42 +
    accent * glow * 0.018;

    float alpha = max(
        mask * 0.86,
        max(border * 0.72, glow * 0.06)
    );
    alpha *= alphaMul * parentMask;

    return vec4(rgb, alpha);
}

// File-count badge.
vec4 makeBadge(float parentMask)
{
    return makePill(
        uBadgePx,
        mix(SPACE_BLUE, STAR_WHITE, 0.18),
        10.0,
        1.02,
        parentMask
    );
}

// Chip pill 0.
vec4 makeChip0(float parentMask)
{
    return makePill(
        uChip0Px,
        GRID_BLUE,
        8.0,
        1.0,
        parentMask
    );
}

// Chip pill 1.
vec4 makeChip1(float parentMask)
{
    return makePill(
        uChip1Px,
        mix(NEBULA, vec3(1.0, 0.20, 0.70), 0.38),
        8.0,
        1.0,
        parentMask
    );
}

// Chip pill 2.
vec4 makeChip2(float parentMask)
{
    return makePill(
        uChip2Px,
        mix(GRID_BLUE, vec3(0.20, 0.95, 1.00), 0.32),
        8.0,
        1.0,
        parentMask
    );
}

void main()
{
    PanelMask panel = makePanelMask();

    vec4 outColor = makePanelColor(panel);
    outColor = blendOver(outColor, makeBadge(panel.shape));
    outColor = blendOver(outColor, makeChip0(panel.shape));
    outColor = blendOver(outColor, makeChip1(panel.shape));
    outColor = blendOver(outColor, makeChip2(panel.shape));

    FragColor = vec4(
        outColor.rgb,
        clamp(outColor.a, 0.0, 0.92)
    );
}