#version 430 core

// Node fragment shader used for rendering
// files and folders in the ChangeTrace scene.
//
// Features:
// - soft circular masking,
// - separate folder/file shading models,
// - rim lighting,
// - specular highlights,
// - glow pulse animation.
in vec2 vLocal;
in vec4 vColor;
in float vGlow;

// Node type.
// 0 = folder
// 1 = file
flat in float vKind;

out vec4 fragColor;

// Global elapsed time in seconds.
// Used for animated glow pulsing.
uniform float uTime;

// Creates soft radial ring mask.
float Ring(float d, float inner, float outer)
{
    return smoothstep(inner, outer, d) -
    smoothstep(outer, 1.0, d);
}

void main()
{
    float d = length(vLocal);

    if (d > 1.0)
    discard;

    vec3 base = vColor.rgb;
    float alphaBase = vColor.a;

    float isFile = step(0.5, vKind);
    float isFolder = 1.0 - isFile;

    float circle = 1.0 - smoothstep(0.96, 1.0, d);
    
    float folderFill = 1.0 - smoothstep(0.88, 1.0, d);
    float folderInner = 1.0 - smoothstep(0.0, 0.45, d);
    float folderBorder = smoothstep(0.68, 0.84, d) - smoothstep(0.94, 1.0, d);
    float folderRim = smoothstep(0.82, 0.96, d) - smoothstep(0.96, 1.0, d);

    vec3 folderColor =
        base * 0.30 +
        base * folderInner * 0.28 +
        base * folderBorder * 1.35 +
        vec3(1.0) * folderRim * 0.18;

    float folderAlpha = max(alphaBase, 0.95) * folderFill * 0.42 +
        folderBorder * 0.72 +
        folderRim * 0.28;

    vec2 lightDir = normalize(vec2(-0.45, 0.75));

    float fileCore = 1.0 - smoothstep(0.0, 0.58, d);
    float fileRim = smoothstep(0.70, 0.92, d) -  smoothstep(0.94, 1.0, d);

    float fileSpec = pow(max(0.0, dot(vLocal, lightDir)), 5.0) *
        (1.0 - smoothstep(0.35, 1.0, d));

    vec3 fileColor =
        base * 0.85 +
        base * fileCore * 0.75 +
        base * fileRim * 1.15 +
        vec3(1.0) * fileSpec * 0.75;

    float fileAlpha = max(alphaBase, 0.92);

    vec3 color = mix(folderColor, fileColor, isFile);
    float alpha = mix(folderAlpha, fileAlpha, isFile) * circle;
    float glow = max(vGlow, 0.0);

    float glowMask = (1.0 - smoothstep(0.25, 1.0, d)) *
        step(0.001, glow);

    float pulse = 1.0 + 0.08 * sin(uTime * 8.0);

    color += base * glow * 1.35 * pulse * glowMask;

    alpha = max(alpha,
        min(1.0, glow * 0.35) * glowMask);

    fragColor = vec4(color, clamp(alpha, 0.0, 1.0));
}