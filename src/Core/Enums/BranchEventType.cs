namespace ChangeTrace.Core.Enums;

/// <summary>
/// Describes the type of branch related repository event.
/// </summary>
public enum BranchEventType
{
    BranchCreated,
    BranchDeleted,
    Merge
}