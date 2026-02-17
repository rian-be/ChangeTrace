using ChangeTrace.Cli.Logging;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.Configuration;

/// <summary>
/// Provides application level initialization for dependency injection and service discovery.
/// </summary>
/// <remarks>
/// This static class offers extension methods to configure the application's <see cref="IServiceCollection"/>.
/// It integrates:
/// <list type="bullet">
/// <item>Automatic discovery and registration of services from the current assembly using <see cref="ServiceDiscoveryExtensions.AddDiscoveredServices"/>.</item>
/// <item>Logging configuration for service discovery diagnostics using <see cref="SpectreConsoleLoggerProvider"/>.</item>
/// <item>Binding of <see cref="ServiceDiscoveryOptions"/> from application configuration (<see cref="IConfiguration"/>).</item>
/// </list>
/// </remarks>
internal static class ApplicationInitialization
{
    /// <summary>
    /// Configures application services, including automatic discovery and DI registration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register services into.</param>
    /// <param name="configuration">The application configuration (<see cref="IConfiguration"/>) used to bind service discovery options.</param>
    /// <param name="logLevel">Minimum log level for service discovery diagnostics (default: <see cref="LogLevel.Information"/>).</param>
    /// <returns>The original <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Creates a logger specifically for service discovery to print debug and informational messages.</item>
    /// <item>Scans the current assembly for concrete classes matching the <see cref="ServiceDiscoveryOptions"/> rules.</item>
    /// <item>Registers discovered services as all their implemented interfaces with the configured lifetime.</item>
    /// <item>Supports configuration overrides via the "ServiceDiscovery" section in <see cref="IConfiguration"/>.</item>
    /// </list>
    /// </remarks>
    internal static IServiceCollection ConfigureApp(
        this IServiceCollection services,
        IConfiguration configuration,
        LogLevel logLevel = LogLevel.Information)
    {
        var discoveryLogger = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new SpectreConsoleLoggerProvider(logLevel));
        }).CreateLogger("ServiceDiscovery");
        
        services.AddDiscoveredServices(opts =>
        {
            configuration.GetSection("ServiceDiscovery").Bind(opts);
        }, discoveryLogger);
        
        return services;
    }
}
