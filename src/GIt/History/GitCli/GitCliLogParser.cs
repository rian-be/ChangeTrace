using System.Runtime.CompilerServices;
using System.Text;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;

namespace ChangeTrace.GIt.History.GitCli;

/// <summary>
/// Parses Git CLI log output into commit domain models.
/// Supports metadata-only and name-status history streams.
/// </summary>
internal static class GitCliLogParser
{
    /// <summary>
    /// Record separator used by git log output.
    /// </summary>
    public const char RecordSeparator = '\x1e';

    /// <summary>
    /// Unit separator used by git log output.
    /// </summary>
    public const char UnitSeparator = '\x1f';

    /// <summary>
    /// Reads commit data from git log output.
    /// </summary>
    public static async IAsyncEnumerable<CommitData> ReadAsync(
        string repositoryPath,
        IReadOnlyList<string> arguments,
        bool includeFileChanges,
        IReadOnlyList<BranchName> branches,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (includeFileChanges)
        {
            await foreach (var commit in ReadNameStatusAsync(
                repositoryPath,
                arguments,
                branches,
                cancellationToken))
            {
                yield return commit;
            }

            yield break;
        }

        await foreach (var commit in ReadMetadataAsync(
           repositoryPath,
           arguments,
           branches,
           cancellationToken))
        {
            yield return commit;
        }
    }

    /// <summary>
    /// Reads commit metadata from git log output.
    /// </summary>
    private static async IAsyncEnumerable<CommitData> ReadMetadataAsync(
        string repositoryPath,
        IReadOnlyList<string> arguments,
        IReadOnlyList<BranchName> branches,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var process = GitCliProcessRunner.Create(repositoryPath, arguments);

        process.Start();

        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        string[]? currentFields = null;

        while (await process.StandardOutput.ReadLineAsync(cancellationToken) is { } line)
        {
            if (line.Length == 0 || line[0] != RecordSeparator)
                continue;

            var pending = MapCommit(currentFields, null, branches);

            if (pending is not null)
                yield return pending;

            currentFields = line[1..].Split(UnitSeparator, 5);
        }

        var finalCommit = MapCommit(currentFields, null, branches);

        if (finalCommit is not null)
            yield return finalCommit;

        await EnsureSuccessAsync(process, errorTask, cancellationToken);
    }

    /// <summary>
    /// Reads commit metadata and file changes from git log output.
    /// </summary>
    private static async IAsyncEnumerable<CommitData> ReadNameStatusAsync(
        string repositoryPath,
        IReadOnlyList<string> arguments,
        IReadOnlyList<BranchName> branches,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var process = GitCliProcessRunner.Create(repositoryPath, arguments);

        process.Start();

        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        string[]? currentFields = null;
        List<FileChange>? currentFileChanges = null;

        string? pendingStatus = null;
        string? pendingOldPath = null;

        await foreach (var token in ReadNullSeparatedTokensAsync(
            process.StandardOutput.BaseStream,
            cancellationToken))
        {
            if (token.Length == 0)
                continue;

            if (LooksLikeCommitToken(token))
            {
                var pending = MapCommit(currentFields, currentFileChanges, branches);

                if (pending is not null)
                    yield return pending;

                var payload = token[1..];
                var statusStart = payload.LastIndexOf('\n');

                if (statusStart >= 0)
                {
                    currentFields = payload[..statusStart].Split(UnitSeparator, 5);
                    pendingStatus = payload[(statusStart + 1)..];
                }
                else
                {
                    currentFields = payload.Split(UnitSeparator, 5);
                    pendingStatus = null;
                }

                currentFileChanges = [];
                pendingOldPath = null;

                continue;
            }

            if (currentFileChanges is null)
                continue;

            if (pendingStatus is null)
            {
                pendingStatus = token;
                continue;
            }

            var kind = MapChangeKind(pendingStatus);

            if (kind == FileChangeKind.Renamed && pendingOldPath is null)
            {
                pendingOldPath = token;
                continue;
            }

            var fileChange = CreateFileChange(
                pendingStatus,
                token,
                kind == FileChangeKind.Renamed
                    ? pendingOldPath
                    : null);

            if (fileChange is not null)
                currentFileChanges.Add(fileChange);

            pendingStatus = null;
            pendingOldPath = null;
        }

        var finalCommit = MapCommit(currentFields, currentFileChanges, branches);

        if (finalCommit is not null)
            yield return finalCommit;

        await EnsureSuccessAsync(process, errorTask, cancellationToken);
    }

    /// <summary>
    /// Verifies successful Git process completion.
    /// </summary>
    private static async Task EnsureSuccessAsync(
        System.Diagnostics.Process process,
        Task<string> errorTask,
        CancellationToken cancellationToken)
    {
        await process.WaitForExitAsync(cancellationToken);

        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(error)
                    ? "Failed to read repository with Git CLI"
                    : error.Trim());
        }
    }

    /// <summary>
    /// Reads null-separated UTF-8 tokens from a stream.
    /// </summary>
    private static async IAsyncEnumerable<string> ReadNullSeparatedTokensAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var buffer = new byte[32768];
        var leftover = new List<byte>();

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer, cancellationToken);

            if (bytesRead == 0)
                break;

            var startIndex = 0;

            while (startIndex < bytesRead)
            {
                var nullIndex = Array.IndexOf(
                    buffer,
                    (byte)0,
                    startIndex,
                    bytesRead - startIndex);

                if (nullIndex >= 0)
                {
                    var length = nullIndex - startIndex;

                    string token;

                    if (leftover.Count > 0)
                    {
                        for (var i = 0; i < length; i++)
                            leftover.Add(buffer[startIndex + i]);

                        token = Encoding.UTF8.GetString(leftover.ToArray());

                        leftover.Clear();
                    }
                    else
                    {
                        token = Encoding.UTF8.GetString(
                            buffer,
                            startIndex,
                            length);
                    }

                    yield return token;

                    startIndex = nullIndex + 1;
                }
                else
                {
                    var length = bytesRead - startIndex;

                    for (var i = 0; i < length; i++)
                        leftover.Add(buffer[startIndex + i]);

                    break;
                }
            }
        }

        if (leftover.Count > 0)
            yield return Encoding.UTF8.GetString(leftover.ToArray());
    }

    /// <summary>
    /// Detects commit record boundaries in token streams.
    /// </summary>
    private static bool LooksLikeCommitToken(string token)
    {
        if (token.Length < 42 || token[0] != RecordSeparator)
            return false;

        for (var index = 1; index <= 40; index++)
        {
            if (!Uri.IsHexDigit(token[index]))
                return false;
        }

        return token[41] == UnitSeparator;
    }

    /// <summary>
    /// Converts parsed git log fields into commit data.
    /// </summary>
    private static CommitData? MapCommit(
        string[]? fields,
        IReadOnlyList<FileChange>? fileChanges,
        IReadOnlyList<BranchName> branches)
    {
        if (fields is null || fields.Length < 5)
            return null;

        var shaResult = CommitSha.Create(fields[0]);
        var authorResult = ActorName.Create(fields[1]);

        var timestampResult = long.TryParse(fields[2], out var unixSeconds)
            ? Timestamp.Create(unixSeconds)
            : Result<Timestamp>.Failure("Invalid commit timestamp");

        if (shaResult.IsFailure ||
            authorResult.IsFailure ||
            timestampResult.IsFailure)
        {
            return null;
        }

        var parentShas = fields[3]
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(CommitSha.Create)
            .Where(result => result.IsSuccess)
            .Select(result => result.Value)
            .ToList();

        fileChanges = parentShas.Count > 0
            ? fileChanges ?? []
            : [];

        return new CommitData(
            Sha: shaResult.Value,
            Author: authorResult.Value,
            Timestamp: timestampResult.Value,
            Message: fields[4],
            ParentShas: parentShas,
            FileChanges: fileChanges,
            Branches: branches,
            IsMerge: parentShas.Count > 1);
    }

    /// <summary>
    /// Creates a file change from parsed git status data.
    /// </summary>
    private static FileChange? CreateFileChange(
        string status,
        string pathValue,
        string? oldPathValue)
    {
        var kind = MapChangeKind(status);

        var pathResult = FilePath.Create(pathValue);

        if (pathResult.IsFailure)
            return null;

        FilePath? oldPath = null;

        if (kind == FileChangeKind.Renamed &&
            oldPathValue is not null)
        {
            var oldPathResult = FilePath.Create(oldPathValue);

            if (oldPathResult.IsSuccess)
                oldPath = oldPathResult.Value;
        }

        return new FileChange(
            Path: pathResult.Value,
            Kind: kind,
            OldPath: oldPath);
    }

    /// <summary>
    /// Converts git status codes into domain change types.
    /// </summary>
    private static FileChangeKind MapChangeKind(string status) =>
        status[0] switch
        {
            'A' => FileChangeKind.Added,
            'D' => FileChangeKind.Deleted,
            'R' => FileChangeKind.Renamed,
            _ => FileChangeKind.Modified
        };
}
