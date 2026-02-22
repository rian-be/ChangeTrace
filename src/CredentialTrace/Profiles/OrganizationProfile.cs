using ChangeTrace.CredentialTrace.Interfaces;

namespace ChangeTrace.CredentialTrace.Profiles;

/// <summary>
/// Represents an organization profile
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IProfile"/>.</item>
/// <item>Tracks creation time and associated authentication session.</item>
/// <item>Holds optional description and repository list.</item>
/// </list>
/// </remarks>
public sealed class OrganizationProfile : IProfile
{
    /// <summary>
    /// Unique identifier of profile.
    /// </summary>
    public Ulid Id { get; set; }

    /// <summary>
    /// Name of organization.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Authentication provider used to create the organization (e.g., github, gitlab).
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp of profile creation.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Session ID of the authenticated user who created this profile.
    /// </summary>
    public Ulid SessionId { get; set; }

    /// <summary>
    /// Optional description of the organization.
    /// </summary>
    public string? Description { get; set; }
}