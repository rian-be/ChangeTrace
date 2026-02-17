using ChangeTrace.Configuration;
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
/// Reads Git repositories using LibGit2Sharp.
/// Performance-focused, minimal allocations.
/// FIXED: Proper branch detection using commit ancestry.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class GitRepositoryReader(ILogger<GitRepositoryReader> logger) : IGitRepositoryReader
{
    /// <summary>
    /// Read all commits from repository
    /// </summary>
    public async Task<Result<IReadOnlyList<CommitData>>> ReadCommitsAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!Repository.IsValid(repositoryPath))
                    return Result<IReadOnlyList<CommitData>>.Failure("Invalid Git repository");

                using var repo = new Repository(repositoryPath);
                
                // Build branch map FIRST - map commit SHA to branches
                var commitToBranches = BuildCommitToBranchMap(repo);
                
                var commits = new List<CommitData>();

                var filter = new CommitFilter
                {
                    SortBy = CommitSortStrategies.Time | CommitSortStrategies.Topological
                };

                int processed = 0;
                foreach (var commit in repo.Commits.QueryBy(filter))
                {
                    if (cancellationToken.IsCancellationRequested)
                        return Result<IReadOnlyList<CommitData>>.Failure("Cancelled");

                    // Apply filters
                    if (options.StartDate.HasValue && commit.Author.When < options.StartDate.Value)
                        continue;

                    if (options.EndDate.HasValue && commit.Author.When > options.EndDate.Value)
                        continue;

                    if (options.MaxCommits > 0 && processed >= options.MaxCommits)
                        break;

                    var commitData = MapCommit(
                        repo, 
                        commit, 
                        options.IncludeFileChanges,
                        commitToBranches
                    );
                    
                    if (commitData.IsSuccess)
                    {
                        commits.Add(commitData.Value);
                        processed++;

                        if (processed % 100 == 0)
                            logger.LogDebug("Processed {Count} commits", processed);
                    }
                }

                logger.LogInformation("Read {Count} commits", commits.Count);
                return Result<IReadOnlyList<CommitData>>.Success(commits);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read repository");
                return Result<IReadOnlyList<CommitData>>.Failure("Failed to read repository", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Build map of commit SHA to branch names.
    /// This is THE FIX for branch detection.
    /// </summary>
    private Dictionary<string, List<string>> BuildCommitToBranchMap(Repository repo)
    {
        var map = new Dictionary<string, List<string>>();

        foreach (var branch in repo.Branches.Where(b => !b.IsRemote))
        {
            if (branch.Tip == null)
                continue;

            var branchName = branch.FriendlyName;
            
            // Add tip commit
            AddToMap(map, branch.Tip.Sha, branchName);

            // For each branch, walk back through its commits
            // This ensures we know which branches contain which commits
            var filter = new CommitFilter
            {
                IncludeReachableFrom = branch.Tip,
                SortBy = CommitSortStrategies.Topological
            };

            // Only walk back a reasonable distance to avoid performance issues
            int walkCount = 0;
            const int maxWalk = 1000; // Configurable limit

            foreach (var commit in repo.Commits.QueryBy(filter))
            {
                AddToMap(map, commit.Sha, branchName);
                
                walkCount++;
                if (walkCount >= maxWalk)
                    break;
            }
        }

        logger.LogDebug("Built branch map with {Count} commits", map.Count);
        return map;
    }

    private void AddToMap(Dictionary<string, List<string>> map, string sha, string branchName)
    {
        if (!map.ContainsKey(sha))
        {
            map[sha] = new List<string>();
        }
        
        if (!map[sha].Contains(branchName))
        {
            map[sha].Add(branchName);
        }
    }

    /// <summary>
    /// Clone remote repository
    /// </summary>
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

                var options = new CloneOptions();
                options.FetchOptions.OnTransferProgress = progress =>
                {
                    if (progress.TotalObjects > 0 && progress.ReceivedObjects % 100 == 0)
                    {
                        logger.LogDebug("Clone progress: {Received}/{Total}",
                            progress.ReceivedObjects, progress.TotalObjects);
                    }
                    return !cancellationToken.IsCancellationRequested;
                };

                Repository.Clone(url, destinationPath, options);

                logger.LogInformation("Clone complete");
                return Result.Success();
            }
            catch (LibGit2SharpException ex)
            {
                logger.LogError(ex, "Clone failed");
                return Result.Failure($"Clone failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Clone failed");
                return Result.Failure("Clone failed", ex);
            }
        }, cancellationToken);
    }

    private Result<CommitData> MapCommit(
        Repository repo, 
        Commit commit, 
        bool includeFileChanges,
        Dictionary<string, List<string>> commitToBranches)
    {
        try
        {
            var shaResult = CommitSha.Create(commit.Sha);
            if (shaResult.IsFailure)
                return Result<CommitData>.Failure(shaResult.Error!);

            var authorResult = ActorName.Create(commit.Author.Name);
            if (authorResult.IsFailure)
                return Result<CommitData>.Failure(authorResult.Error!);

            var timestampResult = Timestamp.Create(commit.Author.When.ToUnixTimeSeconds());
            if (timestampResult.IsFailure)
                return Result<CommitData>.Failure(timestampResult.Error!);

            // Parent SHAs
            var parentShas = commit.Parents
                .Select(p => CommitSha.Create(p.Sha))
                .Where(r => r.IsSuccess)
                .Select(r => r.Value)
                .ToList();

            // File changes (expensive - only if requested)
            var fileChanges = includeFileChanges
                ? GetFileChanges(repo, commit)
                : Array.Empty<FileChange>();

            // FIXED: Get branches from pre-built map
            var branches = new List<BranchName>();
            if (commitToBranches.TryGetValue(commit.Sha, out var branchNames))
            {
                foreach (var branchName in branchNames)
                {
                    var branchResult = BranchName.Create(branchName);
                    if (branchResult.IsSuccess)
                    {
                        branches.Add(branchResult.Value);
                    }
                }
            }

            // If no branches found, try to get HEAD branch
            if (branches.Count == 0)
            {
                try
                {
                    var head = repo.Head;
                    if (head != null && !head.IsRemote)
                    {
                        var branchResult = BranchName.Create(head.FriendlyName);
                        if (branchResult.IsSuccess)
                        {
                            branches.Add(branchResult.Value);
                        }
                    }
                }
                catch
                {
                    // Ignore HEAD detection errors
                }
            }

            // Log if we still have no branches (for debugging)
            if (branches.Count == 0)
            {
                logger.LogTrace("No branches found for commit {Sha}", commit.Sha[..8]);
            }

            var commitData = new CommitData(
                Sha: shaResult.Value,
                Author: authorResult.Value,
                Timestamp: timestampResult.Value,
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

    private IReadOnlyList<FileChange> GetFileChanges(Repository repo, Commit commit)
    {
        try
        {
            var parent = commit.Parents.FirstOrDefault();
            if (parent == null)
                return Array.Empty<FileChange>();

            var changes = repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);
            var fileChanges = new List<FileChange>();

            foreach (var change in changes)
            {
                var pathResult = FilePath.Create(change.Path);
                if (pathResult.IsFailure)
                    continue;

                FilePath? oldPath = null;
                if (change.Status == ChangeKind.Renamed && change.OldPath != null)
                {
                    var oldPathResult = FilePath.Create(change.OldPath);
                    if (oldPathResult.IsSuccess)
                        oldPath = oldPathResult.Value;
                }

                var fileChange = new FileChange(
                    Path: pathResult.Value,
                    Kind: MapChangeKind(change.Status),
                    OldPath: oldPath
                );

                fileChanges.Add(fileChange);
            }

            return fileChanges;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get file changes for {Sha}", commit.Sha[..8]);
            return Array.Empty<FileChange>();
        }
    }

    private static FileChangeKind MapChangeKind(ChangeKind kind) => kind switch
    {
        ChangeKind.Added => FileChangeKind.Added,
        ChangeKind.Modified => FileChangeKind.Modified,
        ChangeKind.Deleted => FileChangeKind.Deleted,
        ChangeKind.Renamed => FileChangeKind.Renamed,
        _ => FileChangeKind.Modified
    };
}