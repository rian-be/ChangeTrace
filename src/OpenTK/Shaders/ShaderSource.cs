namespace ChangeTrace.OpenTK.Shaders
{
    internal static class ShaderSource
    {
        internal const string CircleVert = """
            #version 330 core
            layout(location = 0) in vec2 aPos;
            layout(location = 1) in vec2 aCenter;
            layout(location = 2) in float aRadius;
            layout(location = 3) in vec4 aColor;
            layout(location = 4) in float aGlow;
            uniform mat3 uViewProj;
            out vec2 vLocal;
            out vec4 vColor;
            out float vGlow;
            out float vRadius;
            void main()
            {
                vLocal = aPos;
                vColor = aColor;
                vGlow = aGlow;
                vRadius = aRadius;
                vec2 worldPos = aCenter + aPos * aRadius;
                vec3 ndc = uViewProj * vec3(worldPos, 1.0);
                gl_Position = vec4(ndc.xy, 0.0, 1.0);
            }
            """;

        internal const string CircleFrag = """
            #version 330 core
            in vec2 vLocal; 
            in vec4 vColor;
            in float vGlow; 
            in float vRadius;
            out vec4 fragColor;
            void main()
            {
                float dist = length(vLocal);
                if (dist > 1.0) discard;
                float glowFactor = smoothstep(1.0, 0.8, dist) * vGlow;
                vec3 finalColor = vColor.rgb + glowFactor;
                fragColor = vec4(finalColor, vColor.a);
            }
            """;

        internal const string EdgeVert = """
            #version 330 core
            layout(location = 0) in vec2 aFrom;
            layout(location = 1) in vec2 aTo;
            layout(location = 2) in vec4 aColor;
            layout(location = 3) in float aAlpha;
            layout(location = 4) in float aWidth;
            layout(location = 5) in float aCorner;
            uniform mat3 uViewProj;
            out vec2 vUV;
            out vec4 vColor;
            void main()
            {
                int corner = int(aCorner);
                vec2 dir = normalize(aTo - aFrom);
                vec2 perp = vec2(-dir.y, dir.x);
                vec2 worldPos;
                if (corner == 0) worldPos = aFrom - perp * aWidth;
                else if (corner == 1) worldPos = aFrom + perp * aWidth;
                else if (corner == 2) worldPos = aTo - perp * aWidth;
                else worldPos = aTo + perp * aWidth;
                vec3 ndc = uViewProj * vec3(worldPos, 1.0);
                gl_Position = vec4(ndc.xy, 0.0, 1.0);
                vUV = vec2(float(corner & 1), float(corner >> 1));
                vColor = vec4(aColor.rgb, aColor.a * aAlpha);
            }
            """;

        internal const string EdgeFrag = """
            #version 330 core
            in vec2 vUV;
            in vec4 vColor;
            out vec4 fragColor;
            void main()
            {
                float tipFade = smoothstep(0.0, 0.05, vUV.y) * smoothstep(1.0, 0.95, vUV.y);
                float edgeFade = 1.0 - smoothstep(0.3, 0.5, abs(vUV.x - 0.5) * 2.0);
                float alpha = vColor.a * tipFade * (0.5 + 0.5 * edgeFade);
                fragColor = vec4(vColor.rgb, alpha);
                if (fragColor.a < 0.01) discard;
            }
            """;

        internal const string ParticleVert = """
            #version 330 core
            layout(location = 0) in vec2 aPos;
            layout(location = 1) in vec4 aColor;
            layout(location = 2) in float aSize;
            uniform mat3 uViewProj;
            out vec4 vColor;
            void main()
            {
                vColor = aColor;
                vec3 ndc = uViewProj * vec3(aPos, 1.0);
                gl_Position = vec4(ndc.xy, 0.0, 1.0);
                gl_PointSize = aSize;
            }
            """;

        internal const string ParticleFrag = """
            #version 330 core
            in vec4 vColor;
            out vec4 fragColor;
            void main()
            {
                vec2 c = gl_PointCoord - 0.5;
                float dist = dot(c, c) * 4.0;
                float alpha = max(0.0, 1.0 - dist);
                fragColor = vec4(vColor.rgb, vColor.a * alpha);
                if (fragColor.a < 0.01) discard;
            }
            """;

        internal const string TextVert = """
            #version 330 core
            layout(location = 0) in vec2 aPos;
            layout(location = 1) in vec2 aUV;
            uniform mat3 uViewProj;
            out vec2 vUV;
            void main()
            {
                vUV = aUV;
                vec3 ndc = uViewProj * vec3(aPos, 1.0);
                gl_Position = vec4(ndc.xy, 0.0, 1.0);
            }
            """;

        internal const string TextFrag = """
            #version 330 core
            in vec2 vUV;
            out vec4 fragColor;
            uniform sampler2D uAtlas;
            uniform vec4 uColor;
            void main()
            {
                float a = texture(uAtlas, vUV).r;
                fragColor = vec4(uColor.rgb, uColor.a * a);
                if (fragColor.a < 0.01) discard;
            }
            """;
    }
}