namespace ChangeTrace.Core.Enums;

/// <summary>
/// Represents type of commit related event in repository.
/// </summary>
internal enum CommitEventType
{
    Commit,
    FileAdded,
    FileModified,
    FileDeleted,
    FileRenamed
}