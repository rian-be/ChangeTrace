namespace ChangeTrace.Core;

/// <summary>
/// Specifies the type of change that occurred to a file in a commit.
/// Used to categorize file modifications when building the commit history and timeline events.
/// </summary>
public enum FileChangeKind
{
    Added,
    Modified,
    Deleted,
    Renamed
}