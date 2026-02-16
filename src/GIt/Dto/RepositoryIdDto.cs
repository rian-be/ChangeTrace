namespace ChangeTrace.GIt.Dto;

/// <summary>
/// Represents Git repository identifier.
/// Serves as a minimal DTO for owner + repository name pairing.
/// </summary>
/// <remarks>
/// - `Owner` corresponds to repository owner (user or organization).  
/// - `Name` corresponds to repository name.  
/// </remarks>
internal sealed record RepositoryIdDto(
    string Owner,
    string Name
);