using Microsoft.Extensions.Logging;

namespace ChangeTrace.Configuration.Discovery;

/// <summary>
/// Filters types during service discovery for automatic DI registration.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Detects type category based on namespace markers (DTO, Entity, Repository, Services, etc.).</item>
/// <item>Extracts a feature name from ".Features.{FeatureName}".</item>
/// <item>Extracts layer from the namespace root (Application, Domain, Infrastructure, etc.).</item>
/// <item>Applies <see cref="ServiceDiscoveryOptions"/> including allowed/excluded namespaces, layers, interfaces, exceptions, and types.</item>
/// <item>Logs a clear debug message indicating decision, layer, type category, feature, and full type name.</item>
/// </list>
/// </remarks>
internal static class ServiceDiscoveryFilter
{
    private static readonly (string Key, string Name)[] NamespaceTypes =
    [
        (".Operations", "Operations"),
        (".Services", "Service"),
        (".Repositories", "Repository"),
        (".Validators", "Validator"),
    ];

    /// <summary>
    /// Determines if a type is valid for automatic DI registration.
    /// </summary>
    /// <param name="type">Type to evaluate.</param>
    /// <param name="logger">Logger for debug output.</param>
    /// <param name="opts">Service discovery options controlling inclusion/exclusion rules.</param>
    /// <returns><c>true</c> if the type should be included; otherwise <c>false</c>.</returns>
    internal static bool IsValidServiceType(Type type, ILogger logger, ServiceDiscoveryOptions opts)
    {
        var ns = type.Namespace ?? string.Empty;
        var layer = GetLayer(ns);
        var feature = GetFeature(ns);
        var detectedType = GetDetectedType(ns);

        var include = ShouldInclude(type, ns, layer, opts);

        if (opts.EnableLogging)
        {
            logger.LogDebug("{Decision} Layer({Layer}) Type({TypeKind}) Feature({Feature}): {Type}",
                include ? "Including" : "Skipping",
                layer,
                detectedType,
                feature,
                type.FullName);
        }

        return include;
    }
    
    /// <summary>
    /// Returns the first namespace segment (layer), e.g. "Application", "Domain".
    /// </summary>
    private static string GetLayer(string ns)
        => ns.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Unknown";

    /// <summary>
    /// Detects a type group (DTO, Entity, Service, etc.) based on namespace markers.
    /// </summary>
    private static string GetDetectedType(string ns)
    {
        foreach (var (key, name) in NamespaceTypes)
            if (ns.Contains(key))
                return name;

        return "Allowed";
    }

    /// <summary>
    /// Extracts feature (the part after ".Features.") or returns "Global".
    /// </summary>
    private static string GetFeature(string ns)
    {
        const string segment = ".Features.";
        var idx = ns.IndexOf(segment, StringComparison.Ordinal);
        if (idx < 0) return "Global";

        var start = idx + segment.Length;
        var end = ns.IndexOf('.', start);
        return end > 0 ? ns[start..end] : ns[start..];
    }
    
    /// <summary>
    /// Applies <see cref="ServiceDiscoveryOptions"/> rules to decide inclusion.
    /// </summary>
    private static bool ShouldInclude(Type type, string ns, string layer, ServiceDiscoveryOptions opts)
        => !(opts.SkipInterfaces && type.IsInterface)
           && !(opts.SkipExceptions && typeof(Exception).IsAssignableFrom(type))
           && !opts.ExcludedTypes.Contains(type)
           && !opts.ExcludedNamespaces.Any(ns.Contains)
           && (opts.AllowedNamespaces.Count == 0 || opts.AllowedNamespaces.Any(ns.Contains))
           && (opts.AllowedLayers.Count == 0 || opts.AllowedLayers.Contains(layer));
}