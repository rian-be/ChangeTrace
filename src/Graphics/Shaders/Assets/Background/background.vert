#version 330 core

// Fullscreen quad vertex shader.
// Passes normalized quad coordinates directly
// to fragment shader.
layout(location = 0) in vec2 aPos;

// Screen-space position passed to fragment shader.
// Used as fullscreen UV-style coordinates.
out vec2 vPos;

void main()
{
    vPos = aPos;
    gl_Position = vec4(aPos, 0.0, 1.0);
}