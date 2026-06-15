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
        HashSet<string> probed =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (string start in EnumerateSearchRoots())
        {
            string? found =
                FindAssetsDirFrom(start, probed);

            if (found is not null)
                return found;
        }

        throw new DirectoryNotFoundException(
            "Shader assets directory not found. Probed: " +
            string.Join(", ", probed));
    }

    private static IEnumerable<string> EnumerateSearchRoots()
    {
        HashSet<string> roots =
            new(StringComparer.OrdinalIgnoreCase);

        AddRoot(AppContext.BaseDirectory);
        AddRoot(Environment.CurrentDirectory);
        AddRoot(Path.GetDirectoryName(typeof(ShaderSource).Assembly.Location));

        return roots;

        void AddRoot(string? path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                roots.Add(Path.GetFullPath(path));
        }
    }

    private static string? FindAssetsDirFrom(
        string start,
        ISet<string> probed)
    {
        string current =
            start;

        while (!string.IsNullOrEmpty(current))
        {
            foreach (string candidate in EnumerateCandidates(current))
            {
                probed.Add(candidate);

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

        return null;
    }

    private static IEnumerable<string> EnumerateCandidates(
        string current)
    {
        yield return Path.Combine(
            current,
            "src",
            "Graphics",
            "Shaders",
            "Assets");

        yield return Path.Combine(
            current,
            "Graphics",
            "Shaders",
            "Assets");

        yield return Path.Combine(
            current,
            "Shaders",
            "Assets");

        yield return Path.Combine(
            current,
            "Assets",
            "Shaders");
    }
}
