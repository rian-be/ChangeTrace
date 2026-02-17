using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Configuration.Discovery;

/// <summary>
/// Configuration options for automatic service discovery and DI registration.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Specifies which namespaces and types should be included or excluded during service scanning.</item>
/// <item>Allows controlling the lifetime of discovered services.</item>
/// <item>Can skip interfaces and exception types if not meant to be registered.</item>
/// </list>
/// </remarks>
internal sealed class ServiceDiscoveryOptions
{
    /// <summary>
    /// Namespaces to include in discovery. Only types within these namespaces will be considered.
    /// </summary>
    internal List<string> AllowedNamespaces { get; set; } = [];

    /// <summary>
    /// Namespaces to exclude from discovery, even if listed in <see cref="AllowedNamespaces"/>.
    /// </summary>
    internal List<string> ExcludedNamespaces { get; set; } = [];

    /// <summary>
    /// Specific types to ignore during service registration.
    /// </summary>
    internal List<Type> ExcludedTypes { get; set; } = [];
    
    internal List<string> AllowedLayers { get; set; } = [];

    /// <summary>
    /// If <c>true</c>, interface types are ignored during discovery.
    /// </summary>
    internal bool SkipInterfaces { get; set; } = true;

    /// <summary>
    /// If <c>true</c>, exception types are ignored during discovery.
    /// </summary>
    internal bool SkipExceptions { get; set; } = true;

    /// <summary>
    /// If false, ServiceDiscovery will not log debug messages.
    /// </summary>
    internal bool EnableLogging { get; set; } = true;
    
    /// <summary>
    /// Default service lifetime for all discovered types.
    /// </summary>
    internal ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
}