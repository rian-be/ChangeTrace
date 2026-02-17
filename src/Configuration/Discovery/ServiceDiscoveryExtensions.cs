using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using System.Runtime.Loader;

namespace ChangeTrace.Configuration.Discovery;

/// <summary>
/// Provides extension methods for automatically discovering and registering services
/// into the <see cref="IServiceCollection"/> based on the <see cref="AutoRegisterAttribute"/>.
/// </summary>
internal static class ServiceDiscoveryExtensions
{
    /// <summary>
    /// Scans all loaded assemblies for classes marked with <see cref="AutoRegisterAttribute"/>
    /// and registers them in the provided <see cref="IServiceCollection"/>.  
    /// Supports optional logging of registration details.
    /// </summary>
    /// <param name="services">The service collection to register discovered services into.</param>
    /// <param name="enableLogging">
    /// If <c>true</c>, logs each discovered service and interface mapping.
    /// </param>
    /// <param name="logger">
    /// Optional logger to use for diagnostic messages. If <c>null</c>, a <see cref="NullLogger"/> is used.
    /// </param>
    internal static void AddDiscoveredServices(this IServiceCollection services,
        bool enableLogging,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        LoadAllAssemblies();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var allTypes = assemblies
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return []; }
            })
            .ToList();
        
        var serviceTypes = allTypes
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetCustomAttributes(typeof(AutoRegisterAttribute), false).Any())
            .ToList();

        foreach (var type in serviceTypes)
        {
            var attr = (AutoRegisterAttribute)type.GetCustomAttributes(typeof(AutoRegisterAttribute), false).First();
            
            var interfaces = attr.As?.Length > 0 ? attr.As : type.GetInterfaces();

            foreach (var iface in interfaces)
            {
                services.Add(new ServiceDescriptor(iface, type, attr.Lifetime));

                if (enableLogging)
                    logger.LogInformation("DI Register {Interface} -> {Implementation}", iface.FullName, type.FullName);
            }
        }

        logger.LogInformation("Service discovery registered {Count} services", serviceTypes.Count);
    }

    private static void LoadAllAssemblies()
    {
        var basePath = AppContext.BaseDirectory;
        var dlls = Directory.GetFiles(basePath, "ChangeTrace*.dll");

        foreach (var dll in dlls)
        {
            try
            {
                var name = AssemblyName.GetAssemblyName(dll);
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == name.Name))
                    continue;

                AssemblyLoadContext.Default.LoadFromAssemblyName(name);
            }
            catch { }
        }
    }
}
