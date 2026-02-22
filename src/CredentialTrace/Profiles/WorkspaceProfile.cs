using ChangeTrace.CredentialTrace.Interfaces;

namespace ChangeTrace.CredentialTrace.Profiles;

/// <summary>
/// Represents a workspace profile in ChangeTrace system.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="IProfile"/>.</item>
/// <item>Associated with a parent organization via <see cref="OrganizationId"/>.</item>
/// <item>Contains workspace-specific settings.</item>
/// <item>Provides factory method for creation and method to update settings.</item>
/// </list>
/// </remarks>
internal sealed class WorkspaceProfile : IProfile
{
    /// <summary>
    /// Unique identifier of the workspace.
    /// </summary>
    public Ulid Id { get; set; }

    /// <summary>
    /// Name of the workspace.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Identifier of the parent organization.
    /// </summary>
    public Ulid OrganizationId { get; set; }

    /// <summary>
    /// Workspace-specific settings.
    /// </summary>
    public WorkspaceSettings Settings { get; set; }

    /// <summary>
    /// UTC timestamp of workspace creation.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Default constructor initializing default values.
    /// </summary>
    public WorkspaceProfile()
    {
        Name = string.Empty;
        Settings = WorkspaceSettings.Default();
    }

    /// <summary>
    /// Internal constructor used by <see cref="Create"/> factory.
    /// </summary>
    /// <param name="id">Workspace ID.</param>
    /// <param name="organizationId">Parent organization ID.</param>
    /// <param name="name">Workspace name.</param>
    private WorkspaceProfile(Ulid id, Ulid organizationId, string name)
    {
        Id = id;
        OrganizationId = organizationId;
        Name = name;
        CreatedAt = DateTime.UtcNow;
        Settings = WorkspaceSettings.Default();
    }

    /// <summary>
    /// Factory method to create a new workspace profile.
    /// </summary>
    /// <param name="organizationId">Parent organization ID.</param>
    /// <param name="name">Workspace name.</param>
    /// <returns>Newly created <see cref="WorkspaceProfile"/>.</returns>
    public static WorkspaceProfile Create(Ulid organizationId, string name)
        => new(Ulid.NewUlid(), organizationId, name);

    /// <summary>
    /// Updates workspace settings.
    /// </summary>
    /// <param name="settings">New workspace settings.</param>
    public void UpdateSettings(WorkspaceSettings settings)
        => Settings = settings;
}