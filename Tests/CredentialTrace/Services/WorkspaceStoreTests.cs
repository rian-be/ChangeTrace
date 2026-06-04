using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.CredentialTrace.Services;
using ChangeTrace.Tests.TestDoubles;
using Xunit;

namespace ChangeTrace.Tests.CredentialTrace.Services;

/// <summary>Tests workspace lookup behavior across organization-backed profile stores.</summary>
public sealed class WorkspaceStoreTests
{
    /// <summary>GetByNameOrganization returns workspaces for the named organization ordered by name.</summary>
    [Fact]
    public async Task GetByNameOrganization_ReturnsMatchingWorkspacesOrderedByName()
    {
        var organization = CreateOrganization("Rian BE");
        var otherOrganization = CreateOrganization("Other");
        var beta = WorkspaceProfile.Create(organization.Id, "Beta");
        var alpha = WorkspaceProfile.Create(organization.Id, "Alpha");
        var outside = WorkspaceProfile.Create(otherOrganization.Id, "Outside");
        var store = new WorkspaceStore(
            new InMemoryProfileStore<OrganizationProfile>(organization, otherOrganization),
            new InMemoryProfileStore<WorkspaceProfile>(beta, outside, alpha));

        var workspaces = await store.GetByNameOrganization("rian be");

        Assert.Equal(["Alpha", "Beta"], workspaces.Select(workspace => workspace.Name).ToArray());
    }

    /// <summary>GetByNameOrganization returns an empty list for blank or unknown organization names.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("missing")]
    public async Task GetByNameOrganization_ReturnsEmptyForBlankOrUnknownOrganization(string organizationName)
    {
        var store = new WorkspaceStore(
            new InMemoryProfileStore<OrganizationProfile>(),
            new InMemoryProfileStore<WorkspaceProfile>());

        var workspaces = await store.GetByNameOrganization(organizationName);

        Assert.Empty(workspaces);
    }

    /// <summary>Creates an organization fixture with the supplied name.</summary>
    private static OrganizationProfile CreateOrganization(string name)
        => new()
        {
            Id = Ulid.NewUlid(),
            Name = name,
            Provider = "github",
            CreatedAt = DateTime.UtcNow,
            SessionId = Ulid.NewUlid()
        };
}
