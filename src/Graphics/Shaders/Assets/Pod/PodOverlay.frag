#version 430 core

// Glass hover card fragment shader.
// Renders rounded panel with pointer, border,
// glow, scan sweep and green accents.

in vec2 vUv;
in vec2 vPixel;

uniform vec4 uRectPx; // expanded draw rect
uniform vec4 uPanelPx; // real panel rect: x, y, width, height

uniform vec4 uFillColor;
uniform vec4 uBorderColor;
uniform vec4 uGlowColor;

uniform float uRadius;
uniform float uBorderWidth;
uniform float uGlowSize;
uniform float uTime;

out vec4 FragColor;

// Rounded rectangle SDF.
float roundedRectSdf(vec2 p, vec2 halfSize, float radius)
{
    vec2 q = abs(p) - halfSize + vec2(radius);

    return length(max(q, 0.0)) +
    min(max(q.x, q.y), 0.0) -
    radius;
}

// Triangle SDF used for the pointer.
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

    float s = sign(e0.x * e2.y - e0.y * e2.x);

    vec2 d = min(
        min(
            vec2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
            vec2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
            vec2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)
        )
    );

    return -sqrt(d.x) * sign(d.y);
}

// Soft line band mask.
float lineBand(float value, float center, float width)
{
    return
    1.0 - smoothstep(
    width,
    width + 1.0,
    abs(value - center));
}

void main()
{
    vec2 panelCenter = uPanelPx.xy + uPanelPx.zw * 0.5;
    vec2 panelSize = uPanelPx.zw;
    vec2 p = vPixel - panelCenter;

    // Main rounded panel SDF.
    float rectSdf = roundedRectSdf(
        p,
        panelSize * 0.5,
        uRadius
    );

    // Bottom pointer triangle.
    vec2 pointerA = vec2(-12.0, panelSize.y * 0.5 - 1.0);
    vec2 pointerB = vec2(12.0, panelSize.y * 0.5 - 1.0);
    vec2 pointerC = vec2(0.0, panelSize.y * 0.5 + 15.0);

    float pointerSdf = triangleSdf(
        p,
        pointerA,
        pointerB,
        pointerC
    );

    // Union between panel and pointer.
    float sdf = min(rectSdf, pointerSdf);

    float aa = 1.25;

    float bodyMask = 1.0 - smoothstep(
        0.0,
        aa,
        sdf
    );

    float rectMask = 1.0 - smoothstep(
        0.0,
        aa,
        rectSdf
    );

    float pointerMask = 1.0 - smoothstep(
        0.0,
        aa,
        pointerSdf
    );

    float borderOuter = 1.0 - smoothstep(
        0.0,
        aa,
        sdf
    );

    float borderInner = 1.0 - smoothstep(
        -uBorderWidth,
        -uBorderWidth - aa,
        sdf
    );

    float borderMask = clamp(borderOuter - borderInner, 0.0, 1.0);

    float glowMask = 1.0 - smoothstep(
        0.0,
        uGlowSize,
        max(sdf, 0.0)
    );

    float innerEdge = 1.0 - smoothstep(
        -18.0,
        -2.0,
        sdf
    );

    vec2 panelUv = clamp(
        (vPixel - uPanelPx.xy) / uPanelPx.zw,
        0.0,
        1.0
    );

    float vertical = 1.0 - panelUv.y;
    float pulse = 0.82 + 0.18 * sin(uTime * 3.2);
    float sweepPosition = fract(uTime * 0.18);

    float diagonal = panelUv.x * 0.72 + vertical * 0.28;

    // Animated diagonal scan sweep.
    float sweep = 1.0 - smoothstep(
        0.0,
        0.08,
        abs(diagonal - sweepPosition)
    );

    float topEdge = lineBand(
        panelUv.y,
        0.055,
        0.018
    );

    float bottomEdge = lineBand(
        panelUv.y,
        0.92,
        0.012
    );

    float leftAccent = lineBand(
    panelUv.x,
        0.035,
        0.012
    ) *
    smoothstep(0.15, 0.55, vertical);

    float rightAccent = lineBand(
    panelUv.x,
        0.965,
        0.012
    ) *
    smoothstep(0.15, 0.55, vertical);

    float cornerLight = pow(1.0 - clamp(length(panelUv - vec2(0.08, 0.13)) * 3.2, 0.0, 1.0), 2.0);

    vec3 glassTop = vec3(0.030, 0.085, 0.070);
    vec3 glassBottom = vec3(0.010, 0.022, 0.024);

    vec3 glass = mix(glassBottom, glassTop, vertical);
    glass += vec3(0.010, 0.085, 0.040) * innerEdge * 0.24;
    glass += vec3(0.070, 0.300, 0.130) * sweep * rectMask * 0.10;
    glass += vec3(0.100, 0.420, 0.180) * topEdge * 0.16;
    glass += vec3(0.040, 0.200, 0.080) * bottomEdge * 0.08;
    glass += vec3(0.110, 0.470, 0.200) * (leftAccent + rightAccent) * 0.16;
    glass += vec3(0.080, 0.280, 0.130) * cornerLight * 0.18;

    float fillAlpha = uFillColor.a * bodyMask;

    vec3 borderRgb = mix(
        vec3(0.070, 0.320, 0.150),
        vec3(0.250, 1.000, 0.440),
        0.42 + topEdge * 0.38 + pulse * 0.12
    );

    float borderAlpha = borderMask * (0.58 + 0.28 * pulse);

    float halo = glowMask * (0.11 + 0.13 * pulse);
    vec3 haloRgb = uGlowColor.rgb * halo;

    // Pointer gets a bit brighter at the tip.
    float pointerTip = pointerMask *
    smoothstep(
        panelSize.y * 0.5,
        panelSize.y * 0.5 + 15.0,
    p.y);

    glass += vec3(0.080, 0.360, 0.150) * pointerTip * 0.18;

    vec3 rgb =
    haloRgb +
    glass * fillAlpha +
    borderRgb * borderAlpha;

    float alpha = max(halo * 0.70, max(fillAlpha, borderAlpha));
    FragColor = vec4(rgb, clamp(alpha, 0.0, 1.0));
}