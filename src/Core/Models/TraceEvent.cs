using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Models;

/// <summary>
/// Rich domain model for a single Git event.
/// Contains behavior, not just data.
/// </summary>
internal sealed class TraceEvent
{
    // Core properties
    public Timestamp Timestamp { get; private set; }
    public ActorName Actor { get; }
    public string Target { get; }
    public string? Metadata { get; private set; }
    
    // Type safe optional properties
    public CommitSha? CommitSha { get; }
    public BranchName? BranchName { get; }
    public PullRequestNumber? PullRequestNumber { get; private set; }
    public FilePath? FilePath { get; }
    
    // Event types
    public CommitEventType? CommitType { get; }
    public BranchEventType? BranchType { get; }
    public PullRequestEventType? PrType { get; private set; }

    private TraceEvent(
        Timestamp timestamp,
        ActorName actor,
        string target,
        string? metadata,
        CommitSha? commitSha,
        BranchName? branchName,
        PullRequestNumber? prNumber,
        FilePath? filePath,
        CommitEventType? commitType,
        BranchEventType? branchType,
        PullRequestEventType? prType)
    {
        Timestamp = timestamp;
        Actor = actor;
        Target = target;
        Metadata = metadata;
        CommitSha = commitSha;
        BranchName = branchName;
        PullRequestNumber = prNumber;
        FilePath = filePath;
        CommitType = commitType;
        BranchType = branchType;
        PrType = prType;
    }

    /// <summary>
    /// Factory: Create commit event
    /// </summary>
    public static TraceEvent CreateCommit(
        Timestamp timestamp,
        ActorName actor,
        CommitSha commitSha,
        string? message = null)
    {
        return new TraceEvent(
            timestamp: timestamp,
            actor: actor,
            target: commitSha.Value,
            metadata: message,
            commitSha: commitSha,
            branchName: null,
            prNumber: null,
            filePath: null,
            commitType: CommitEventType.Commit,
            branchType: null,
            prType: null
        );
    }

    /// <summary>
    /// Factory: Create file change event
    /// </summary>
    public static TraceEvent CreateFileChange(
        Timestamp timestamp,
        ActorName actor,
        FilePath filePath,
        CommitEventType changeType,
        CommitSha commitSha,
        string? metadata = null)
    {
        return new TraceEvent(
            timestamp: timestamp,
            actor: actor,
            target: filePath.Value,
            metadata: metadata,
            commitSha: commitSha,
            branchName: null,
            prNumber: null,
            filePath: filePath,
            commitType: changeType,
            branchType: null,
            prType: null
        );
    }

    /// <summary>
    /// Factory: Create branch event
    /// </summary>
    public static TraceEvent CreateBranch(
        Timestamp timestamp,
        ActorName actor,
        BranchName branchName,
        BranchEventType branchType,
        CommitSha? commitSha = null,
        string? metadata = null)
    {
        return new TraceEvent(
            timestamp: timestamp,
            actor: actor,
            target: branchName.Value,
            metadata: metadata,
            commitSha: commitSha,
            branchName: branchName,
            prNumber: null,
            filePath: null,
            commitType: null,
            branchType: branchType,
            prType: null
        );
    }

    /// <summary>
    /// Factory: Create merge commit event
    /// </summary>
    public static TraceEvent CreateMerge(
        Timestamp timestamp,
        ActorName actor,
        CommitSha commitSha,
        BranchName? targetBranch = null,
        string? message = null)
    {
        return new TraceEvent(
            timestamp: timestamp,
            actor: actor,
            target: commitSha.Value,
            metadata: message,
            commitSha: commitSha,
            branchName: targetBranch,
            prNumber: null,
            filePath: null,
            commitType: CommitEventType.Commit,
            branchType: BranchEventType.Merge,
            prType: null
        );
    }

    /// <summary>
    /// Enrich event with PR data (mutation for performance)
    /// </summary>
    public Result EnrichWithPullRequest(
        PullRequestNumber prNumber,
        PullRequestEventType prType,
        string? additionalMetadata = null)
    {
        if (PullRequestNumber.HasValue)
            return Result.Failure("Event already has PR data");

        PullRequestNumber = prNumber;
        PrType = prType;
        
        if (additionalMetadata != null)
            Metadata = CombineMetadata(Metadata, additionalMetadata);

        return Result.Success();
    }

    /// <summary>
    /// Normalize timestamp relative to base time
    /// </summary>
    public void NormalizeTime(Timestamp baseTime)
    {
        Timestamp = Timestamp.Normalize(baseTime);
    }

    /// <summary>
    /// Check if this is a merge commit
    /// </summary>
    public bool IsMergeCommit() => BranchType == BranchEventType.Merge;

    /// <summary>
    /// Check if event has PR data
    /// </summary>
    public bool HasPullRequest() => PrType.HasValue;

    /// <summary>
    /// Get human-readable event type
    /// </summary>
    public string GetEventType() => (PrType, BranchType, CommitType) switch
    {
        (not null, _, _) => $"PR:{PrType}",
        (_, not null, _) => $"Branch:{BranchType}",
        (_, _, not null) => $"Commit:{CommitType}",
        _ => "Unknown"
    };

    /// <summary>
    /// Check if target matches (supports partial SHA matching)
    /// </summary>
    public bool MatchesTarget(string target)
    {
        if (Target.Equals(target, StringComparison.Ordinal))
            return true;

        // Partial SHA matching
        if (CommitSha != null && target.Length >= 7)
        {
            var targetShaResult = CommitSha.Create(target);
            if (targetShaResult.IsSuccess)
                return CommitSha.Matches(targetShaResult.Value);
        }

        return false;
    }

    private static string CombineMetadata(string? existing, string newMetadata)
    {
        if (string.IsNullOrEmpty(existing))
            return newMetadata;
        
        return $"{existing} | {newMetadata}";
    }

    public override string ToString() => 
        $"[{Timestamp}] {Actor}: {GetEventType()} on {Target}";
}