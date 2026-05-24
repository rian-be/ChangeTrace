#version 430 core

// Heatmap texture fragment shader.
// Applies optional soft upscale, intensity, gamma,
// contrast, saturation and color grading.

in vec2 vUv;

out vec4 fragColor;

uniform sampler2D uTexture;

uniform float uOpacity;
uniform float uIntensity;
uniform float uGamma;
uniform float uContrast;
uniform float uSaturation;
uniform int uUseSoftUpscale;

const vec3 LumaWeights = vec3(
0.2126,
0.7152,
0.0722);

// Adjusts color saturation using luma.
vec3 SaturateColor(vec3 color, float saturation)
{
    float luma = dot(color, LumaWeights);
    return mix(vec3(luma), color, saturation);
}

// Applies subtle cinematic heatmap tint.
vec3 ApplyColorGrade(vec3 color)
{
    color *= vec3(1.08, 1.02, 1.22);
    float brightness = dot(color, vec3(0.333333));

    vec3 tint = mix(
        vec3(0.00, 0.02, 0.08),
        vec3(0.08, 0.02, 0.12),
        smoothstep(
            0.20,
            0.90,
            brightness
        )
    );

    return color + tint;
}

// Samples texture with small 3x3 weighted blur.
// Useful when upscaling low resolution heatmap.
vec4 SampleSoft(sampler2D tex, vec2 uv)
{
    vec2 texel =1.0 / vec2(textureSize(tex, 0));
    vec4 center = texture(tex, uv);

    vec4 axis =
    texture(tex, uv + vec2( texel.x, 0.0)) +
    texture(tex, uv + vec2(-texel.x, 0.0)) +
    texture(tex, uv + vec2(0.0,  texel.y)) +
    texture(tex, uv + vec2(0.0, -texel.y));

    vec4 diagonal =
    texture(tex, uv + vec2( texel.x,  texel.y)) +
    texture(tex, uv + vec2(-texel.x,  texel.y)) +
    texture(tex, uv + vec2( texel.x, -texel.y)) +
    texture(tex, uv + vec2(-texel.x, -texel.y));

    return center * 0.40 + axis * 0.12 + diagonal * 0.03;
}

// Applies final post processing to heatmap RGB.
vec3 ApplyPostProcess(vec3 color)
{
    color *= uIntensity;

    color = pow(max(
        color,
        vec3(0.0)),
        vec3(1.0 / max(
            uGamma,
            0.001
        ))
    );

    color = (color - 0.5) * uContrast + 0.5;
    color = SaturateColor(color, uSaturation);
    return ApplyColorGrade(color);
}

void main()
{
    vec4 color = uUseSoftUpscale != 0
    ? SampleSoft(uTexture, vUv)
    : texture(uTexture, vUv);

    if (color.a <= 0.001)
    discard;

    color.rgb = ApplyPostProcess(color.rgb);
    color.a *= uOpacity;
    fragColor = color;
}