using ChangeTrace.Core.Models;

namespace ChangeTrace.Core;

/// <summary>
/// Represents a file change within a commit.
/// Captures the path, type of change, and optional original path for renamed files.
/// Used to track file-level modifications across the commit history.
/// </summary>
/// <param name="Path">The current path of the file in the repository.</param>
/// <param name="Kind">The type of change (Added, Modified, Deleted, Renamed).</param>
/// <param name="OldPath">The original path before the change. Only populated for renamed files (Kind = Renamed).</param>
internal sealed record FileChange(
    FilePath Path,
    FileChangeKind Kind,
    FilePath? OldPath = null
);