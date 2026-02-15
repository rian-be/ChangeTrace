namespace ChangeTrace.Core.Models;

/// <summary>
/// Base type for domain value objects.
/// 
/// Serves as  marker for immutable, self-contained objects
/// that are compared by value rather than by reference.
/// </summary>
public abstract record ValueObject;