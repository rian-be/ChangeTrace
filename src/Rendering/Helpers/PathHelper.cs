namespace ChangeTrace.Rendering.Helpers;

/// <summary>
/// Utility helpers for repository-style path operations.
/// </summary>
internal static class PathHelper
{
    /// <summary>
    /// Resolves parent directory path.
    /// </summary>
    internal static string GetParentPath(string path)
    {
        path = path.TrimStart('/');
        int idx = path.LastIndexOf('/');

        if (idx < 0)
            return "";

        return path.Substring(0, idx);
    }
}