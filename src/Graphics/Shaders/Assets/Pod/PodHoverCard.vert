#version 430 core

// Screen-space hover card vertex shader.
// Converts pixel space rectangle into clip space quad vertices.

layout(location = 0) in vec2 aLocal;

uniform vec4 uRectPx;
uniform vec2 uViewport;

out vec2 vUv;
out vec2 vPixel;

void main()
{
    // Local quad UV in [0, 1].
    vUv = aLocal;

    // Pixel space position inside the target rectangle.
    vec2 px = uRectPx.xy + aLocal * uRectPx.zw;
    vPixel = px;

    // Convert pixel coordinates to normalized device coordinates.
    vec2 ndc = vec2(
        px.x / uViewport.x * 2.0 - 1.0,
        1.0 - px.y / uViewport.y * 2.0
    );

    gl_Position = vec4(ndc, 0.0, 1.0);
}