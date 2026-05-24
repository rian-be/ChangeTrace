#version 430 core

// Simple pulse fragment shader.
// Modulates alpha intensity over time.

uniform vec4 uColor;
uniform float uTime;

out vec4 FragColor;

void main()
{
    // Animated pulse intensity.
    float pulse = 0.78 + 0.22 * sin(uTime * 4.0);

    FragColor = vec4(
    uColor.rgb,
    uColor.a * pulse
    );
}