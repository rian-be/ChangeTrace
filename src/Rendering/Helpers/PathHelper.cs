namespace ChangeTrace.Rendering.Helpers;

internal static class PathHelper
{ 
    internal static string GetParentPath(string path)
    {
        int idx = path.LastIndexOf('/');
        if (idx < 0) return "";
        return path.Substring(0, idx);
    }
}