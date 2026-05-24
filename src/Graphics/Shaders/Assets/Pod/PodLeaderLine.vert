#version 430 core

// Screen-space line/shape vertex shader.
// Converts pixel space positions into clip space coordinates.

layout(location = 0) in vec2 aPositionPx;

uniform vec2 uViewport;

void main()
{
    // Convert pixel coordinates into normalized device coordinates.
    vec2 ndc = vec2(
        aPositionPx.x / uViewport.x * 2.0 - 1.0,
        1.0 - aPositionPx.y / uViewport.y * 2.0
    );

    gl_Position = vec4(ndc, 0.0, 1.0);
}