namespace ChangeTrace.GIt.Delegates;

/// <summary>
/// Represents progress callback for reporting operation status during long-running processes.
/// Used throughout the export pipeline to provide visibility into cloning, reading, building, and enrichment phases.
/// </summary>
/// <param name="stage">Current stage of the operation (e.g., "Prepare", "Read", "Build", "Enrich", "Save").</param>
/// <param name="current">Current step number within the current stage (1-based).</param>
/// <param name="total">Total number of steps within the current stage.</param>
/// <param name="message">Optional human-readable status message describing the current operation.</param>
public delegate void ProgressCallback(string stage, int current, int total, string? message = null);