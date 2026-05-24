#version 430 core

// Edge fragment shader.
// Renders soft glowing edge strip with:
// - smooth edge fading,
// - bright center core,
// - additive glow shaping.

in vec4 vColor;

// Edge alpha multiplier.
// Usually driven by edge activity or visibility.
in float vAlpha;

// Signed edge side coordinate.
// -1 = left edge
//  1 = right edge
in float vSide;

out vec4 fragColor;

void main()
{
    // Distance from edge center line.
    float distanceFromCenter = abs(vSide);

    // Soft fade near outer strip edges.
    float edgeFade = 1.0 - smoothstep(
        0.82,
        1.0,
        distanceFromCenter);

    // Bright center highlight.
    float core = 1.0 - smoothstep(
        0.0,
        0.28,
        distanceFromCenter);

    // Wide soft glow profile.
    float glow = 1.0 -
        distanceFromCenter *
        distanceFromCenter;

    vec3 color = vColor.rgb;

    // White hot center boost.
    color += vec3(1.0) * core * 0.35;

    // Colored outer glow.
    color += vColor.rgb * glow * 0.25;

    float alpha =
        vColor.a *
        vAlpha *
        edgeFade *
        glow;

    fragColor = vec4(color, alpha);
}