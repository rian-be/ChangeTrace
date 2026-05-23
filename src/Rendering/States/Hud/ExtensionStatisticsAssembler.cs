using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.States.Hud;

/// <summary>
/// Builds aggregated file extension statistics for the HUD.
/// </summary>
internal sealed class ExtensionStatisticsAssembler
{
    /// <summary>
    /// Collects and ranks the most common file extensions in the scene.
    /// </summary>
    public IReadOnlyList<ExtensionStat> Assemble(ISceneGraph scene)
    {
        var counts = new Dictionary<string, int>();

        foreach (var node in scene.Nodes.Values)
        {
            if (node.Kind != NodeKind.File)
                continue;

            if (string.IsNullOrWhiteSpace(node.Extension))
                continue;

            counts[node.Extension] =
                counts.GetValueOrDefault(node.Extension) + 1;
        }

        return counts
            .OrderByDescending(x => x.Value)
            .Take(8)
            .Select(x => new ExtensionStat(x.Key, x.Value))
            .ToList();
    }
}