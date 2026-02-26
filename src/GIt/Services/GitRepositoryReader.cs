using System.Collections.Concurrent;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Services;


/// <summary>
/// Service for reading Git repositories using <c>LibGit2Sharp</c>.
/// Provides commit history, branch mapping, and optional file changes.
/// </summary>
/// <remarks>
/// Implements <see cref="IGitRepositoryReader"/>.  
/// Branch detection uses commit ancestry to ensure correctness.  
/// Clone operations supported with logging and progress reporting.
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class GitRepositoryReader(ILogger<GitRepositoryReader> logger) : IGitRepositoryReader
{
    /// <summary>
    /// Reads commits from repository with optional file change tracking.
    /// </summary>
    /// <param name="repositoryPath">Path to local Git repository.</param>
    /// <param name="options">Reader options, including date range, max commits, and file change inclusion.</param>
    /// <param name="cancellationToken">Token to cancel operation.</param>
    /// <returns>
    /// Result containing readonly list of <see cref="CommitData"/> on success, 
    /// or failure with error message.
    /// </returns>
    public async Task<Result<IReadOnlyList<CommitData>>> ReadCommitsAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(async () =>
        {
            if (!Repository.IsValid(repositoryPath))
                return Result<IReadOnlyList<CommitData>>.Failure("Invalid Git repository");

            try
            {
                using var repo = new Repository(repositoryPath);
                var commitToBranches = BuildCommitToBranchMap(repo);
                var commitResults = new ConcurrentBag<CommitData>();
                var filter = new CommitFilter {  SortBy = CommitSortStrategies.Time | CommitSortStrategies.Topological };

                var commits = repo.Commits.QueryBy(filter)
                    .Where(c => IsCommitInRange(c, options))
                    .Take(options.MaxCommits > 0 ? options.MaxCommits : int.MaxValue)
                    .ToList();
                var semaphore = new SemaphoreSlim(4);

                var tasks = commits.Select(async commit =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        var result = MapCommit(repo, commit, options.IncludeFileChanges, commitToBranches);
                        if (result.IsSuccess)
                            commitResults.Add(result.Value);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToArray();
                
                await Task.WhenAll(tasks);
                
                var orderedCommits = commitResults
                    .OrderBy(c => c.Timestamp.UnixSeconds)
                    .ToList()
                    .AsReadOnly();

                logger.LogInformation("Read {Count} commits", orderedCommits.Count);
                return Result<IReadOnlyList<CommitData>>.Success(orderedCommits);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read repository");
                return Result<IReadOnlyList<CommitData>>.Failure("Failed to read repository", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Determines if commit is within configured date range.
    /// </summary>
    private static bool IsCommitInRange(Commit commit, GitReaderOptions options)
    {
        return (!options.StartDate.HasValue || commit.Author.When >= options.StartDate.Value)
               && (!options.EndDate.HasValue || commit.Author.When <= options.EndDate.Value);
    }
    
    /// <summary>
    /// Builds mapping from commit SHA to branch names.
    /// </summary>
    /// <param name="repo">Repository to analyze.</param>
    /// <returns>Dictionary mapping commit SHA to list of branches containing it.</returns>
    private Dictionary<string, List<string>> BuildCommitToBranchMap(Repository repo)
    {
        var map = new Dictionary<string, List<string>>();
        const int maxWalk = 1000;
        
        foreach (var branch in repo.Branches.Where(b => !b.IsRemote && b.Tip != null))
        {
            var branchName = branch.FriendlyName;
            AddToMap(map, branch.Tip.Sha, branchName);

            foreach (var (commit, index) in repo.Commits
                         .QueryBy((new CommitFilter { IncludeReachableFrom = branch.Tip, SortBy = CommitSortStrategies.Topological }))
                         .Select((c, i) => (c, i)))
            {
                AddToMap(map, commit.Sha, branchName);
                if (index + 1 >= maxWalk) break;
            }
        }

        logger.LogDebug("Built branch map with {Count} commits", map.Count);
        return map;
    }
    
    /// <summary>
    /// Adds commit SHA to the branch mapping.
    /// </summary>
    private static void AddToMap(Dictionary<string, List<string>> map, string sha, string branchName) =>
        (map.TryGetValue(sha, out var list) ? list : map[sha] = [])
            .Add(branchName);
    
    /// <summary>
    /// Clones remote repository to local path.
    /// </summary>
    /// <param name="url">Repository URL.</param>
    /// <param name="destinationPath">Destination folder for clone.</param>
    /// <param name="cancellationToken">Token to cancel operation.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> CloneAsync(
        string url,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                logger.LogInformation("Cloning {Url} to {Path}", url, destinationPath);

                if (Directory.Exists(destinationPath))
                    Directory.Delete(destinationPath, true);

                Directory.CreateDirectory(destinationPath);

                var options = new CloneOptions
                {
                    FetchOptions =
                    {
                        OnTransferProgress = progress =>
                        {
                            if (progress.TotalObjects > 0 && progress.ReceivedObjects % 100 == 0)
                            {
                                logger.LogDebug("Clone progress: {Received}/{Total}",
                                    progress.ReceivedObjects, progress.TotalObjects);
                            }
                            return !cancellationToken.IsCancellationRequested;
                        }
                    }
                };
                Repository.Clone(url, destinationPath, options);
                
                logger.LogInformation("Clone complete");
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Clone failed");
                return Result.Failure("Clone failed", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Maps <see cref="Commit"/> to <see cref="CommitData"/> object.
    /// </summary>
    /// <param name="repo">Repository containing the commit.</param>
    /// <param name="commit">Commit to map.</param>
    /// <param name="includeFileChanges">Whether to include file level changes.</param>
    /// <param name="commitToBranches">Precomputed commit-to-branch mapping.</param>
    /// <returns>Result containing <see cref="CommitData"/> on success.</returns>
    private Result<CommitData> MapCommit(
        Repository repo, 
        Commit commit, 
        bool includeFileChanges,
        Dictionary<string, List<string>> commitToBranches)
    {
        try
        {
            var basicsResult = MapCommitBasics(commit);
            if (basicsResult.IsFailure)
                return Result<CommitData>.Failure(basicsResult.Error!);

            var (sha, author, ts, parentShas) = basicsResult.Value;
            var fileChanges = includeFileChanges
                ? GetFileChanges(repo, commit) : [];
            var branches = GetBranches(commit, repo, commitToBranches);

            if (!branches.Any())
                logger.LogTrace("No branches found for commit {Sha}", commit.Sha[..8]);

            var commitData = new CommitData(
                Sha: sha,
                Author: author,
                Timestamp: ts,
                Message: commit.MessageShort,
                ParentShas: parentShas,
                FileChanges: fileChanges,
                Branches: branches,
                IsMerge: commit.Parents.Count() > 1
            );

            return Result<CommitData>.Success(commitData);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to map commit {Sha}", commit.Sha[..8]);
            return Result<CommitData>.Failure($"Failed to map commit {commit.Sha[..8]}", ex);
        }
    }
    
    /// <summary>
    /// Maps basic commit data: SHA, author, timestamp, and parent SHAs.
    /// </summary>
    /// <param name="commit">Commit to map.</param>
    /// <returns>
    /// Result containing tuple of commit SHA, author name, timestamp, and parent SHAs on success.
    /// </returns>
    private static Result<(CommitSha sha, ActorName author, Timestamp ts, List<CommitSha> parents)> MapCommitBasics(Commit commit)
    {
        var shaResult = CommitSha.Create(commit.Sha);
        if (shaResult.IsFailure) return Result<(CommitSha, ActorName, Timestamp, List<CommitSha>)>.Failure(shaResult.Error!);

        var authorResult = ActorName.Create(commit.Author.Name);
        if (authorResult.IsFailure) return Result<(CommitSha, ActorName, Timestamp, List<CommitSha>)>.Failure(authorResult.Error!);

        var timestampResult = Timestamp.Create(commit.Author.When.ToUnixTimeSeconds());
        if (timestampResult.IsFailure) return Result<(CommitSha, ActorName, Timestamp, List<CommitSha>)>.Failure(timestampResult.Error!);

        var parentShas = commit.Parents
            .Select(p => CommitSha.Create(p.Sha))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value)
            .ToList();

        return Result<(CommitSha, ActorName, Timestamp, List<CommitSha>)>.Success(
            (shaResult.Value, authorResult.Value, timestampResult.Value, parentShas)
        );
    }
    
    /// <summary>
    /// Retrieves branches containing given commit.
    /// Falls back to HEAD if no mapping found.
    /// </summary>
    /// <param name="commit">Commit to locate branches for.</param>
    /// <param name="repo">Repository context.</param>
    /// <param name="commitToBranches">Precomputed commit to branch mapping.</param>
    /// <returns>List of <see cref="BranchName"/> instances.</returns>
    private List<BranchName> GetBranches(Commit commit, Repository repo, Dictionary<string, List<string>> commitToBranches)
    {
        if (commitToBranches.TryGetValue(commit.Sha, out var branchNames))
        {
            return branchNames
                .Select(BranchName.Create)
                .Where(r => r.IsSuccess)
                .Select(r => r.Value)
                .ToList();
        }

        // fallback to HEAD
        try
        {
            var head = repo.Head;
            if (head != null && !head.IsRemote)
            {
                var branchResult = BranchName.Create(head.FriendlyName);
                if (branchResult.IsSuccess) return [branchResult.Value];
            }
        }
        catch
        {
            // ignore HEAD errors
        }

        return [];
    }
    
    /// <summary>
    /// Retrieves file level changes for commit.
    /// </summary>
    /// <param name="repo">Repository context.</param>
    /// <param name="commit">Commit to inspect.</param>
    /// <returns>Readonly list of <see cref="FileChange"/> objects.</returns>
    private IReadOnlyList<FileChange> GetFileChanges(Repository repo, Commit commit)
    {
        try
        {
            var parent = commit.Parents.FirstOrDefault();
            if (parent == null)
                return [];

            var changes = repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);
            var fileChanges = new List<FileChange>();

            foreach (var change in changes)
            {
                var pathResult = FilePath.Create(change.Path);
                if (pathResult.IsFailure)
                    continue;

                FilePath? oldPath = null;
                if (change is { Status: ChangeKind.Renamed, OldPath: not null })
                {
                    var oldPathResult = FilePath.Create(change.OldPath);
                    if (oldPathResult.IsSuccess)
                        oldPath = oldPathResult.Value;
                }
                
                fileChanges.Add( new FileChange(
                    Path: pathResult.Value,
                    Kind: MapChangeKind(change.Status),
                    OldPath: oldPath
                ));
            }

            return fileChanges.AsReadOnly();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get file changes for {Sha}", commit.Sha[..8]);
            return [];
        }
    }

    /// <summary>
    /// Maps <see cref="ChangeKind"/> from LibGit2Sharp to <see cref="FileChangeKind"/>.
    /// </summary>
    /// <param name="kind">LibGit2Sharp change kind.</param>
    /// <returns>Corresponding <see cref="FileChangeKind"/> value.</returns>
    private static FileChangeKind MapChangeKind(ChangeKind kind) => kind switch
    {
        ChangeKind.Added => FileChangeKind.Added,
        ChangeKind.Modified => FileChangeKind.Modified,
        ChangeKind.Deleted => FileChangeKind.Deleted,
        ChangeKind.Renamed => FileChangeKind.Renamed,
        _ => FileChangeKind.Modified
    };
}