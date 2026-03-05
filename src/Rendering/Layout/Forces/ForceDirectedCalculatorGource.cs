using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Layout.Proximity;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Forces;

/// <summary>
/// Force calculator for Gource-like layout (efficient, multi-repo, folder-aware).
/// </summary>
internal sealed class ForceDirectedCalculatorGource(INodeProximity proximity) : IForceCalculator
{
    public float RepulsionStrength { get; set; } = 8_000f;
    public float SpringStrength { get; set; } = 0.05f;
    public float SpringLength { get; set; } = 120f;
    public float Gravity { get; set; } = 0.02f;

    public Dictionary<string, Vec2> CalculateForces(IReadOnlyList<SceneNode> nodes)
    {
        var forces = new Dictionary<string, Vec2>();
        foreach (var node in nodes)
            forces[node.Id] = Vec2.Zero;

        // --- Repulsion & Spring forces (pairwise only for connected nodes) ---
        for (int i = 0; i < nodes.Count; i++)
        for (int j = i + 1; j < nodes.Count; j++)
        {
            var a = nodes[i];
            var b = nodes[j];

            // Repulsion (all nodes)
            var delta = a.Position - b.Position;
            float distSq = delta.LengthSq + 0.01f;
            var repulsion = delta.Normalized() * (RepulsionStrength / distSq);
            forces[a.Id] += repulsion;
            forces[b.Id] -= repulsion;

            // Spring force (only for connected nodes)
            if (!proximity.AreConnected(a, b)) continue;
            float dist = delta.Length + 0.01f;
            var spring = delta.Normalized() * (SpringStrength * (dist - SpringLength));
            forces[a.Id] += spring;
            forces[b.Id] -= spring;
        }

        // --- Folder-aware layout ---
        float repoOffsetX = 0f;
        const float fileSpacing = 50f;
        const float folderSpacing = 120f;
        const float repoSpacing = 500f;

        // Grupy repo root
        var repoRoots = nodes.Where(n => n.Kind == NodeKind.Root)
                             .OrderBy(n => n.Id)
                             .ToList();

        foreach (var root in repoRoots)
        {
            // Węzły w tym repo
            var repoNodes = nodes.Where(n => n.Id.StartsWith(root.Id))
                                 .ToList();

            // Grupowanie po folderach
            var folderGroups = repoNodes.GroupBy(n => GetParentPath(n))
                                        .OrderBy(g => g.Key);

            foreach (var folder in folderGroups)
            {
                var orderedNodes = folder.OrderBy(n => n.Id).ToList();
                for (int i = 0; i < orderedNodes.Count; i++)
                {
                    var node = orderedNodes[i];
                    int depth = GetDepth(node);
                    float x = repoOffsetX + i * fileSpacing;
                    float y = -depth * folderSpacing;
                    Vec2 target = new Vec2(x, y);
                    forces[node.Id] += (target - node.Position) * Gravity;
                }
            }

            repoOffsetX += repoSpacing;
        }

        return forces;
    }

    private static string GetParentPath(SceneNode node)
    {
        int lastSlash = node.Id.LastIndexOf('/');
        if (lastSlash < 0) return "";
        return node.Id.Substring(0, lastSlash);
    }

    private static int GetDepth(SceneNode node)
    {
        return node.Id.Count(c => c == '/');
    }
}