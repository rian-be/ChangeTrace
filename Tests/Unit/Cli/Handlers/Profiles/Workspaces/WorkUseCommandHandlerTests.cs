using ChangeTrace.Cli.Commands.Profiles.Workspaces;
using ChangeTrace.Cli.Handlers.Profiles.Workspaces;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.Tests.TestDoubles;
using Xunit;

namespace ChangeTrace.Tests.Cli.Handlers.Profiles.Workspaces;

/// <summary>Tests non-interactive workspace selection logic for the work use CLI handler.</summary>
public sealed class WorkUseCommandHandlerTests
{
    /// <summary>HandleAsync selects the named workspace inside the named organization.</summary>
    [Fact]
    public async Task HandleAsync_WithOrganizationAndWorkspaceArguments_SetsCurrentWorkspace()
    {
        var organization = CreateOrganization("Rian BE");
        var workspace = WorkspaceProfile.Create(organization.Id, "Backend");
        var context = new RecordingWorkspaceContext();
        var handler = new WorkUseCommandHandler(
            new InMemoryProfileStore<OrganizationProfile>(organization),
            new InMemoryProfileStore<WorkspaceProfile>(workspace),
            context);
        var command = new WorkUseCommand().Build();
        var parseResult = command.Parse(["Rian BE", "Backend"]);

        await handler.HandleAsync(parseResult, CancellationToken.None);

        Assert.Same(workspace, context.Current);
    }

    /// <summary>Creates an organization fixture for CLI workspace tests.</summary>
    private static OrganizationProfile CreateOrganization(string name)
        => new()
        {
            Id = Ulid.NewUlid(),
            Name = name,
            Provider = "github",
            CreatedAt = DateTime.UtcNow,
            SessionId = Ulid.NewUlid()
        };

    /// <summary>Workspace context test double that records the active workspace.</summary>
    private sealed class RecordingWorkspaceContext : IWorkspaceContext
    {
        /// <summary>Currently selected workspace.</summary>
        public WorkspaceProfile? Current { get; private set; }

        /// <summary>Records the workspace passed by the handler.</summary>
        public Task SetCurrentAsync(WorkspaceProfile workspace, CancellationToken ct = default)
        {
            Current = workspace;
            return Task.CompletedTask;
        }
    }
}
