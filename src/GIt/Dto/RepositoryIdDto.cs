using MessagePack;

namespace ChangeTrace.GIt.Dto;

/// <summary>
/// Represents Git repository identifier.
/// Serves as a minimal DTO for owner + repository name pairing.
/// </summary>
/// <remarks>
/// - `Owner` corresponds to repository owner (user or organization).  
/// - `Name` corresponds to repository name.  
/// </remarks>
[MessagePackObject(AllowPrivate = true)]
internal sealed record RepositoryIdDto([property: Key(0)] string Owner, [property: Key(1)] string Name);