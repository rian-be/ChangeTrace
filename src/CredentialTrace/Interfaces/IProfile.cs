namespace ChangeTrace.CredentialTrace.Interfaces;

/// <summary>
/// Profile with unique identifier and name.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Represents user or configuration entity in the system.</item>
/// <item>ID is used for unique identification of profile.</item>
/// <item>Name is used for display and lookup.</item>
/// </list>
/// </remarks>
internal interface IProfile
{
    /// <summary>
    /// Unique identifier of profile.
    /// </summary>
    Ulid Id { get; }

    /// <summary>
    /// Profile name.
    /// </summary>
    string Name { get; }
}