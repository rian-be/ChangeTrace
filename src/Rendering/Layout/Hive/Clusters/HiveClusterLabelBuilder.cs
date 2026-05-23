using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Clusters;

/// <summary>
/// Generates readable labels for hive clusters.
/// </summary>
internal sealed class HiveClusterLabelBuilder
{
    /// <summary>
    /// Builds display label for the cluster.
    /// </summary>
    public string Build(
        SceneNode parent,
        IReadOnlyList<SceneNode> files,
        int clusterIndex)
    {
        string? token =
            FindDominantPathToken(
                parent,
                files);

        if (!string.IsNullOrWhiteSpace(token))
            return $"{token} · {files.Count}";

        if (!string.IsNullOrWhiteSpace(parent.Label))
        {
            return
                $"{parent.Label} · {clusterIndex + 1} · {files.Count}";
        }

        return $"cluster {clusterIndex + 1} · {files.Count}";
    }

    /// <summary>
    /// Finds dominant semantic token shared by files.
    /// </summary>
    private static string? FindDominantPathToken(
        SceneNode parent,
        IReadOnlyList<SceneNode> files)
    {
        if (files.Count == 0)
            return null;

        Dictionary<string, int> counts =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (SceneNode file in files)
        {
            foreach (string token in ExtractPathTokens(parent, file))
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                if (token.Length <= 1)
                    continue;

                if (IsWeakToken(token))
                    continue;

                counts[token] =
                    counts.GetValueOrDefault(token) + 1;
            }
        }

        if (counts.Count == 0)
            return null;

        KeyValuePair<string, int> best =
            counts
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key.Length)
                .ThenBy(x => x.Key)
                .First();

        if (best.Value <= 1 &&
            files.Count > 4)
        {
            return null;
        }

        return best.Key;
    }

    /// <summary>
    /// Extracts normalized semantic path tokens from a file.
    /// </summary>
    private static IEnumerable<string> ExtractPathTokens(
        SceneNode parent,
        SceneNode file)
    {
        string raw =
            !string.IsNullOrWhiteSpace(file.Label)
                ? file.Label
                : file.Id;

        raw =
            raw.Replace('\\', '/');

        string parentLabel =
            parent.Label.Replace('\\', '/');

        if (!string.IsNullOrWhiteSpace(parentLabel) &&
            raw.StartsWith(
                parentLabel,
                StringComparison.OrdinalIgnoreCase))
        {
            raw =
                raw[parentLabel.Length..]
                    .TrimStart('/');
        }

        string[] parts =
            raw.Split(
                ['/', '\\', '.', '_', '-', ' ', ':'],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        foreach (string part in parts)
        {
            string normalized =
                NormalizeToken(part);

            if (!string.IsNullOrWhiteSpace(normalized))
                yield return normalized;
        }
    }

    /// <summary>
    /// Normalizes semantic token.
    /// </summary>
    private static string NormalizeToken(string token)
    {
        token =
            token.Trim();

        if (token.Length == 0)
            return "";

        if (token.Length > 28)
            token = token[..28];

        return token.ToLowerInvariant();
    }

    /// <summary>
    /// Determines whether a token is too generic.
    /// </summary>
    private static bool IsWeakToken(string token)
    {
        return token is
            "src" or
            "source" or
            "include" or
            "lib" or
            "libs" or
            "file" or
            "files" or
            "test" or
            "tests" or
            "txt" or
            "md" or
            "cc" or
            "cpp" or
            "cxx" or
            "h" or
            "hpp" or
            "cs" or
            "json" or
            "yaml" or
            "yml" or
            "xml" or
            "html" or
            "css" or
            "js" or
            "ts" or
            "tsx";
    }
}