using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Configuration;

/// <summary>
/// Marks a class for automatic DI registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class AutoRegisterAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped, params Type[]? @as)
    : Attribute
{
    /// <summary>
    /// Specifies the service lifetime for this type (default: Scoped).
    /// </summary>
    public ServiceLifetime Lifetime { get; } = lifetime;

    /// <summary>
    /// Optional: specify interfaces to register as. If null, registers all implemented interfaces.
    /// </summary>
    public Type[]? As { get; } = @as;
}