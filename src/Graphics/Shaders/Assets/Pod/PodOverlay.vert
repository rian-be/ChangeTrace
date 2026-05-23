#version 430 core

// Screen space hover card vertex shader.
// Converts padded pixel rect into clip space coordinates.

layout(location = 0) in vec2 aLocal;

uniform vec4 uRectPx; // draw rect with glow padding: x, y, width, height
uniform vec2 uViewport;

out vec2 vUv;
out vec2 vPixel;

void main()
{
    // Local quad UV in [0, 1].
    vUv = aLocal;

    // Pixel space position inside draw rect.
    vec2 px = uRectPx.xy + aLocal * uRectPx.zw;
    vPixel = px;

    // Convert pixel coordinates into NDC.
    vec2 ndc = vec2(
        px.x / uViewport.x * 2.0 - 1.0,
        1.0 - px.y / uViewport.y * 2.0
    );
    gl_Position = vec4(ndc, 0.0, 1.0);
}