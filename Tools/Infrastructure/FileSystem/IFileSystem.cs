namespace ChangeTrace.Tools.Infrastructure.FileSystem;

/// <summary>
/// Abstraction for physical file system operations.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Ensures that the parent directory for the given path exists.
    /// </summary>
    void EnsureParentDirectory(string path);
}