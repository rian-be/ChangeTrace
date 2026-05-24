using ChangeTrace.Graphics.Rendering.Renderers.Sprites;

namespace ChangeTrace.Graphics.Rendering.Text;

/// <summary>
/// Loads the generated language icon atlas used by HUD sprite rendering.
/// </summary>
internal static class LanguageIconAtlasLoader
{
    private const string AtlasFileName =
        "language_icons.png";

    /// <summary>
    /// Loads the icon atlas renderer if the atlas file exists.
    /// </summary>
    internal static IconSpriteRenderer? LoadOrNull()
    {
        string[] candidatePaths =
            GetCandidatePaths()
                .Distinct()
                .ToArray();

        foreach (string path in candidatePaths)
        {
            if (!File.Exists(path))
                continue;

            IconSpriteRenderer renderer =
                IconSpriteRenderer.LoadFromPng(path);

            Console.WriteLine(
                $"[IconSpriteRenderer] Loaded language icon atlas: {path}");

            return renderer;
        }

        Console.WriteLine(
            "[IconSpriteRenderer] language_icons.png not found. Tried:");

        foreach (string path in candidatePaths)
        {
            Console.WriteLine(
                $"  - {path}");
        }

        return null;
    }

    /// <summary>
    /// Resolves possible language icon atlas locations.
    /// </summary>
    private static IEnumerable<string> GetCandidatePaths()
    {
        yield return Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            AtlasFileName);

        yield return Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "Assets",
                AtlasFileName));

        yield return Path.Combine(
            Directory.GetCurrentDirectory(),
            "Assets",
            AtlasFileName);

        yield return Path.Combine(
            AppContext.BaseDirectory,
            AtlasFileName);
    }
}
