#version 430 core

// Screen space textured quad vertex shader.
// Converts pixel-space rectangles into clip space coordinates
// and interpolates UV coordinates.

layout(location = 0) in vec2 aLocal;

// Rectangle in pixel space.
// xy = top-left position
// zw = size
uniform vec4 uRectPx;

// Texture UV rectangle.
// xy = UV origin
// zw = UV size
uniform vec4 uUvRect;

// Current viewport size in pixels.
uniform vec2 uViewport;

out vec2 vUv;

void main()
{
    // Local quad vertex converted into pixel-space position.
    vec2 px = uRectPx.xy + aLocal * uRectPx.zw;

    // Interpolated texture coordinates.
    vUv = uUvRect.xy + aLocal * uUvRect.zw;

    // Convert pixel coordinates into normalized device coordinates.
    vec2 ndc = vec2(
        px.x / uViewport.x * 2.0 - 1.0,
        1.0 - px.y / uViewport.y * 2.0
    );

    gl_Position = vec4(ndc, 0.0, 1.0);
}