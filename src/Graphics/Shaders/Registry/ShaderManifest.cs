namespace ChangeTrace.Graphics.Shaders.Registry;

/// <summary>
/// Static registry containing all runtime shader definitions.
/// </summary>
internal static class ShaderManifest
{
    /// <summary>
    /// Registered runtime shader definitions.
    /// </summary>
    public static readonly ShaderDefinition[] All =
    [
        new("Circle", ShaderKind.Graphics, "Circle/circle.vert", "Circle/circle.frag", null),
        new("CircleCull", ShaderKind.Compute, null, null, "Circle/circle_cull.comp"),

        new("Edge", ShaderKind.Graphics, "Edge/edge.vert", "Edge/edge.frag", null),
        new("EdgeCull", ShaderKind.Compute, null, null, "Edge/edge_cull.comp"),

        new("Particle", ShaderKind.Graphics, "Particle/particle.vert", "Particle/particle.frag", null),
        new("ParticleCull", ShaderKind.Compute, null, null, "Particle/particle_cull.comp"),

        new("Pawn", ShaderKind.Graphics, "Pawn/pawn.vert", "Pawn/pawn.frag", null),
        new("PawnCull", ShaderKind.Compute, null, null, "Pawn/pawn_cull.comp"),

        new("Text", ShaderKind.Graphics, "Text/text.vert", "Text/text.frag", null),
        new("TextCull", ShaderKind.Compute, null, null, "Text/text_cull.comp"),
        
        new("Background", ShaderKind.Graphics, "Background/background.vert", "Background/background.frag", null),
        new("HeatmapTexture", ShaderKind.Graphics, "Heatmap/heatmap_texture.vert", "Heatmap/heatmap_texture.frag", null),
        new("Heatmap", ShaderKind.Compute, null, null, "Heatmap/heatmap.comp"),
        
        new("PodLeaderLine", ShaderKind.Graphics, "Pod/PodLeaderLine.vert", "Pod/PodLeaderLine.frag", null),
        new("PodOverlay", ShaderKind.Graphics, "Pod/PodOverlay.vert", "Pod/PodOverlay.frag", null),
        new("PodHoverCard", ShaderKind.Graphics, "Pod/PodHoverCard.vert", "Pod/PodHoverCard.frag", null),
        
        
        new("IconSprite", ShaderKind.Graphics, "Icon/IconSprite.vert", "Icon/IconSprite.frag", null),
        
        new("BloomMask", ShaderKind.Compute, null, null, "PostProcess/bloom_mask.comp"),
    ];
}