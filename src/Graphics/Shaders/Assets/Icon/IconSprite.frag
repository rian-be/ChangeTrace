#version 430 core

// Simple textured fragment shader.
// Samples texture and applies tint color.

in vec2 vUv;

// Source texture.
uniform sampler2D uTexture;

// RGBA tint multiplier.
// RGB affects color.
// A affects final transparency.
uniform vec4 uTint;

out vec4 FragColor;

void main()
{
    // Sample texture color.
    vec4 tex = texture(uTexture, vUv);

    // Apply tint and alpha modulation.
    FragColor = vec4(
        tex.rgb * uTint.rgb,
        tex.a * uTint.a
    );
}