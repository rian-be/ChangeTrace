using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Events;

/// <summary>
/// Rich domain model for a single Git event.
/// Contains behavior, not just data.
/// </summary>
internal sealed class TraceEvent
{
    // Core properties
    internal Timestamp Timestamp { get; private set; }
    internal ActorName Actor { get; }
    public string Target { get; }
    public string? Metadata { get; private set; }
    
    // Type safe optional properties
    public CommitSha? CommitSha { get; }
    public BranchName? BranchName { get; }
    public PullRequestNumber? PullRequestNumber { get; private set; }
    
    public List<ActorName>? Reviewers { get; private set; }
    public ActorName? MergedBy { get; private set; }  
    
    public List<ActorName>? Contributors { get; private set; }
    public Dictionary<ActorName, Timestamp>? LastModified { get; private set; }
    
    public FilePath? FilePath { get; }
    
    // Event types
    public CommitEventType? CommitType { get; }
    public BranchEventType? BranchType { get; }
    public PullRequestEventType? PrType { get; private set; }

    internal TraceEvent(
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
    /// Enrich event with PR data (mutation for performance)
    /// </summary>
    internal Result EnrichWithPullRequest(
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
    internal void NormalizeTime(Timestamp baseTime)
    {
        Timestamp = Timestamp.Normalize(baseTime);
    }

    internal void AddContributor(ActorName actor, Timestamp time)
    {
        if (Contributors == null) Contributors = new List<ActorName>();
        if (!Contributors.Contains(actor)) Contributors.Add(actor);

        if (LastModified == null) LastModified = new Dictionary<ActorName, Timestamp>();
        LastModified[actor] = time;
    }
    
    /// <summary>
    /// Check if this is a merge commit
    /// </summary>
    internal bool IsMergeCommit() => BranchType == BranchEventType.Merge;

    /// <summary>
    /// Check if event has PR data
    /// </summary>
    internal bool HasPullRequest() => PrType.HasValue;

    /// <summary>
    /// Get human-readable event type
    /// </summary>
    internal string GetEventType() => (PrType, BranchType, CommitType) switch
    {
        (not null, _, _) => $"PR:{PrType}",
        (_, not null, _) => $"Branch:{BranchType}",
        (_, _, not null) => $"Commit:{CommitType}",
        _ => "Unknown"
    };

    /// <summary>
    /// Check if target matches (supports partial SHA matching)
    /// </summary>
    internal bool MatchesTarget(string target)
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