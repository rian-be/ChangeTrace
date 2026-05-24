namespace ChangeTrace.Graphics.Shaders;

/// <summary>
/// Loads shader source files from shader asset directories.
/// </summary>
internal static class ShaderSource
{
    private static string? _assetsDir;

    /// <summary>
    /// Loads shader source file contents.
    /// </summary>
    internal static string Load(string relativePath)
    {
        string assetsDir =
            _assetsDir ??= FindAssetsDir();

        string path =
            Path.Combine(
                assetsDir,
                relativePath);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Shader file not found: {path}");
        }

        return File.ReadAllText(path);
    }

    /// <summary>
    /// Locates shader asset root directory.
    /// </summary>
    private static string FindAssetsDir()
    {
        string current =
            AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(current))
        {
            string[] candidates =
            [
                Path.Combine(
                    current,
                    "src",
                    "Graphics",
                    "Shaders",
                    "Assets"),

                Path.Combine(
                    current,
                    "Graphics",
                    "Shaders",
                    "Assets"),

                Path.Combine(
                    current,
                    "Shaders",
                    "Assets")
            ];

            foreach (string candidate in candidates)
            {
                if (Directory.Exists(candidate))
                    return candidate;
            }

            string? parent =
                Directory.GetParent(current)?.FullName;

            if (parent == null ||
                parent == current)
            {
                break;
            }

            current = parent;
        }

        throw new DirectoryNotFoundException(
            "Shader assets directory not found.");
    }
}