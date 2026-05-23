#version 430 core

// Fullscreen textured quad vertex shader.
// Passes UV coordinates directly to the fragment stage.

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aUv;

out vec2 vUv;

void main()
{
    // Forward texture UVs.
    vUv = aUv;

    // Fullscreen clip  space position.
    gl_Position = vec4(
    aPosition,
    0.0,
    1.0);
}