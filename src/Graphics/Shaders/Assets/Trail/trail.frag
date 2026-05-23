#version 330 core

// Simple glow/orb fragment shader.
// Renders soft radial energy glow with animated pulsing.

in vec2 vPos;
out vec4 fragColor;

// Global elapsed time in seconds.
// Used for brightness pulsing.
uniform float time;

void main()
{
    // Squared radial distance from center.
    float distSq = dot(vPos, vPos);

    // Exponential radial glow falloff.
    float intensity = exp(-distSq * 2.5);

    // Skip nearly invisible pixels.
    if (intensity < 0.01)
    discard;

    // Subtle animated pulse.
    intensity *= 0.85 + 0.15 * sin(time * 2.0);

    // Hot inner glow blended into softer outer color.
    vec3 color = mix(
    vec3(1.0, 0.4, 0.0),
    vec3(1.0, 0.9, 0.3),
    intensity);

    fragColor = vec4(color, intensity);
}