using System.Text.Json;
using System.Text.RegularExpressions;
using ChangeTrace.Configuration.Converters;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Services;

/// <summary>
/// Stores timeline files and metadata for workspaces.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkspaceTimelineStorage : IWorkspaceTimelineStorage
{
    private const string TimelineExtension = ".gittrace";
    private const string MetadataExtension = ".metadata.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new UlidJsonConverter() }
    };

    private readonly string _storageRoot;
    private readonly IProfileStore<OrganizationProfile>? _organizationStore;

    /// <summary>
    /// Creates torage service rooted at the application directory.
    /// </summary>
    public WorkspaceTimelineStorage(IProfileStore<OrganizationProfile> organizationStore)
        : this(AppContext.BaseDirectory, organizationStore)
    {
    }

    /// <summary>
    /// Creates storage service for the specified root directory.
    /// </summary>
    internal WorkspaceTimelineStorage(
        string storageRoot,
        IProfileStore<OrganizationProfile>? organizationStore = null)
    {
        _storageRoot = storageRoot;
        _organizationStore = organizationStore;
    }

    /// <inheritdoc />
    public async Task<string> CreateTimelinePathAsync(
        WorkspaceProfile workspace,
        string repositorySource,
        DateTimeOffset exportedAt,
        string uniqueId,
        CancellationToken ct = default)
    {
        var repository = ResolveRepository(repositorySource);

        var repositoryDirectory = repository.Owner is null
            ? Slug(repository.Name)
            : $"{Slug(repository.Owner)}-{Slug(repository.Name)}";

        var organizationDirectory = await ResolveOrganizationDirectoryAsync(workspace, ct);
        var workspaceDirectory = Slug(workspace.Name);

        var fileName =
            $"{exportedAt.UtcDateTime:yyyyMMddTHHmmssZ}-{Slug(uniqueId)}{TimelineExtension}";

        return Path.Combine(
            _storageRoot,
            "workspaces",
            organizationDirectory,
            workspaceDirectory,
            "timelines",
            repositoryDirectory,
            fileName);
    }

    /// <inheritdoc />
    public async Task SaveMetadataAsync(
        string timelinePath,
        WorkspaceProfile workspace,
        string repositorySource,
        DateTimeOffset exportedAt,
        CancellationToken ct = default)
    {
        var repository = ResolveRepository(repositorySource);

        var metadata = new WorkspaceTimelineMetadata
        {
            WorkspaceId = workspace.Id,
            WorkspaceName = workspace.Name,
            RepositorySource = repositorySource,
            RepositoryOwner = repository.Owner,
            RepositoryName = repository.Name,
            ExportedAtUtc = exportedAt.ToUniversalTime(),
            TimelinePath = timelinePath
        };

        var metadataPath = GetMetadataPath(timelinePath);
        var directory = Path.GetDirectoryName(metadataPath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(metadata, JsonOptions);
        await File.WriteAllTextAsync(metadataPath, json, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WorkspaceTimelineFile>> ListTimelinesAsync(
        WorkspaceProfile workspace,
        CancellationToken ct = default)
    {
        var root = await GetWorkspaceTimelineRootAsync(workspace, ct);

        if (!Directory.Exists(root))
            return [];

        return Directory
            .EnumerateFiles(
                root,
                "*" + TimelineExtension,
                SearchOption.AllDirectories)
            .Select(CreateTimelineFile)
            .OrderByDescending(file =>
                file.Metadata?.ExportedAtUtc ?? file.LastModifiedUtc)
            .ToList();
    }

    /// <summary>
    /// Gets the timeline root directory for a workspace.
    /// </summary>
    private async Task<string> GetWorkspaceTimelineRootAsync(
        WorkspaceProfile workspace,
        CancellationToken ct)
    {
        return Path.Combine(
            _storageRoot,
            "workspaces",
            await ResolveOrganizationDirectoryAsync(workspace, ct),
            Slug(workspace.Name),
            "timelines");
    }

    /// <summary>
    /// Resolves the organization directory name for a workspace.
    /// </summary>
    private async Task<string> ResolveOrganizationDirectoryAsync(
        WorkspaceProfile workspace,
        CancellationToken ct)
    {
        if (_organizationStore == null)
            return Slug(workspace.OrganizationId.ToString());

        var organization = await _organizationStore.GetAsync(workspace.OrganizationId, ct);

        return organization == null
            ? Slug(workspace.OrganizationId.ToString())
            : Slug(organization.Name);
    }

    /// <summary>
    /// Creates timeline file metadata from a file path.
    /// </summary>
    private static WorkspaceTimelineFile CreateTimelineFile(string path)
    {
        var info = new FileInfo(path);

        return new WorkspaceTimelineFile
        {
            Path = path,
            FileName = info.Name,
            SizeBytes = info.Exists ? info.Length : 0,
            LastModifiedUtc = info.Exists
                ? info.LastWriteTimeUtc
                : DateTimeOffset.MinValue,
            Metadata = LoadMetadata(path)
        };
    }

    /// <summary>
    /// Loads timeline metadata if available.
    /// </summary>
    private static WorkspaceTimelineMetadata? LoadMetadata(string timelinePath)
    {
        var metadataPath = GetMetadataPath(timelinePath);

        if (!File.Exists(metadataPath))
            return null;

        try
        {
            var json = File.ReadAllText(metadataPath);
            return JsonSerializer.Deserialize<WorkspaceTimelineMetadata>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets metadata file path for a timeline.
    /// </summary>
    private static string GetMetadataPath(string timelinePath)
        => timelinePath + MetadataExtension;

    /// <summary>
    /// Resolves repository information from a source string.
    /// </summary>
    private static RepositoryDescriptor ResolveRepository(string source)
    {
        if (TryResolveRemoteRepository(source, out var remote))
            return remote;

        var name = source.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        name = Path.GetFileName(name);

        if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];

        return new RepositoryDescriptor(
            null,
            string.IsNullOrWhiteSpace(name) ? "repository" : name);
    }

    /// <summary>
    /// Attempts to resolve repository owner and name from a remote URL.
    /// </summary>
    private static bool TryResolveRemoteRepository(
        string source,
        out RepositoryDescriptor repository)
    {
        repository = default;

        var isRemote =
            source.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            source.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            source.StartsWith("git@", StringComparison.OrdinalIgnoreCase);

        if (!isRemote)
            return false;

        try
        {
            var url = source.Replace(
                "git@github.com:",
                "https://github.com/");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            var segments = uri.AbsolutePath
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
                return false;

            var name = segments[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase)
                ? segments[1][..^4]
                : segments[1];

            repository = new RepositoryDescriptor(segments[0], name);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a value into a filesystem-safe slug.
    /// </summary>
    private static string Slug(string value)
    {
        var slug = Regex.Replace(
            value.Trim().ToLowerInvariant(),
            @"[^a-z0-9._-]+",
            "-");

        slug = slug.Trim('-', '.', '_');

        return string.IsNullOrWhiteSpace(slug)
            ? "unknown"
            : slug;
    }

    /// <summary>
    /// Represents repository owner and name.
    /// </summary>
    private readonly record struct RepositoryDescriptor(
        string? Owner,
        string Name);
}
