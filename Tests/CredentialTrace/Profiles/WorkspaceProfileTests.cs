using ChangeTrace.CredentialTrace.Profiles;
using Xunit;

namespace ChangeTrace.Tests.CredentialTrace.Profiles;

/// <summary>Tests workspace profile creation and settings updates.</summary>
public sealed class WorkspaceProfileTests
{
    /// <summary>Create assigns identity, organization ownership, settings, and timestamp defaults.</summary>
    [Fact]
    public void Create_InitializesDefaults()
    {
        var organizationId = Ulid.NewUlid();

        var workspace = WorkspaceProfile.Create(organizationId, "Backend");

        Assert.NotEqual(default, workspace.Id);
        Assert.Equal(organizationId, workspace.OrganizationId);
        Assert.Equal("Backend", workspace.Name);
        Assert.NotNull(workspace.Settings);
        Assert.True(workspace.CreatedAt <= DateTime.UtcNow);
    }

    /// <summary>UpdateSettings replaces the workspace settings instance.</summary>
    [Fact]
    public void UpdateSettings_ReplacesSettings()
    {
        var workspace = WorkspaceProfile.Create(Ulid.NewUlid(), "Backend");
        var settings = new WorkspaceSettings
        {
            DefaultBranch = "develop",
            AutoSync = false,
            Environment = "prod"
        };

        workspace.UpdateSettings(settings);

        Assert.Same(settings, workspace.Settings);
    }
}
