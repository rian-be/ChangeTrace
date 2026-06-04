using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.CredentialTrace.Services;
using ChangeTrace.Tests.TestDoubles;
using Xunit;

namespace ChangeTrace.Tests.CredentialTrace.Services;

/// <summary>Tests workspace timeline path generation and metadata discovery.</summary>
public sealed class WorkspaceTimelineStorageTests
{
    /// <summary>CreateTimelinePathAsync builds a stable organization/workspace/repository timeline path.</summary>
    [Fact]
    public async Task CreateTimelinePath_UsesOrganizationWorkspaceRepositoryAndUniqueFileName()
    {
        var root = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests");
        var organization = CreateOrganization("Microsoft");
        var workspace = WorkspaceProfile.Create(organization.Id, "MsQuic");
        var storage = new WorkspaceTimelineStorage(root, new InMemoryProfileStore<OrganizationProfile>(organization));
        var exportedAt = new DateTimeOffset(2026, 5, 30, 12, 34, 56, TimeSpan.Zero);

        var path = await storage.CreateTimelinePathAsync(
            workspace,
            "https://github.com/microsoft/msquic.git",
            exportedAt,
            "01JY0000000000000000000000");

        var expected = Path.Combine(
            root,
            "workspaces",
            "microsoft",
            "msquic",
            "timelines",
            "microsoft-msquic",
            "20260530T123456Z-01jy0000000000000000000000.gittrace");

        Assert.Equal(expected, path);
    }

    /// <summary>CreateTimelinePathAsync derives the repository slug from a local repository path.</summary>
    [Fact]
    public async Task CreateTimelinePath_UsesLocalRepositoryNameWhenSourceIsPath()
    {
        var root = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests");
        var organization = CreateOrganization("Local Org");
        var workspace = WorkspaceProfile.Create(organization.Id, "Local Workspace");
        var storage = new WorkspaceTimelineStorage(root, new InMemoryProfileStore<OrganizationProfile>(organization));
        var exportedAt = new DateTimeOffset(2026, 5, 30, 12, 34, 56, TimeSpan.Zero);

        var path = await storage.CreateTimelinePathAsync(
            workspace,
            "/tmp/repos/My Repo.git",
            exportedAt,
            "export-1");

        Assert.EndsWith(
            Path.Combine(
                "workspaces",
                "local-org",
                "local-workspace",
                "timelines",
                "my-repo",
                "20260530T123456Z-export-1.gittrace"),
            path);
    }

    /// <summary>Saved metadata is returned with the matching workspace timeline listing.</summary>
    [Fact]
    public async Task SaveMetadataAndListTimelines_ReturnsTimelineWithMetadata()
    {
        var root = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Ulid.NewUlid().ToString());
        var organization = CreateOrganization("Rian BE");
        var workspace = WorkspaceProfile.Create(organization.Id, "Prod");
        var storage = new WorkspaceTimelineStorage(root, new InMemoryProfileStore<OrganizationProfile>(organization));
        var exportedAt = new DateTimeOffset(2026, 6, 3, 18, 45, 0, TimeSpan.Zero);
        var path = await storage.CreateTimelinePathAsync(
            workspace,
            "git@github.com:rian-be/ChangeTrace.git",
            exportedAt,
            "export-1");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, "gittrace");
        await storage.SaveMetadataAsync(path, workspace, "git@github.com:rian-be/ChangeTrace.git", exportedAt);

        var timelines = await storage.ListTimelinesAsync(workspace);

        var timeline = Assert.Single(timelines);
        Assert.Equal(path, timeline.Path);
        Assert.Equal("rian-be", timeline.Metadata?.RepositoryOwner);
        Assert.Equal("ChangeTrace", timeline.Metadata?.RepositoryName);
        Assert.Equal(exportedAt, timeline.Metadata?.ExportedAtUtc);
    }

    /// <summary>Creates a GitHub organization profile fixture with the supplied display name.</summary>
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
