namespace ChangeTrace.CredentialTrace.Profiles;

public sealed class WorkspaceSettings
{
    public string? DefaultBranch { get; init; }
    public bool AutoSync { get; init; }
    public string? Environment { get; init; }

    public static WorkspaceSettings Default()
        => new()
        {
            DefaultBranch = "main",
            AutoSync = true,
            Environment = "dev"
        };
}