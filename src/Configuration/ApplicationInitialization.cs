using ChangeTrace.Cli.Logging;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Rendering.Camera;
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
/// <item>Automatic discovery and registration of services using <see cref="ServiceDiscoveryExtensions.AddDiscoveredServices"/>.</item>
/// <item>Logging configuration for service discovery diagnostics using <see cref="SpectreConsoleLoggerProvider"/>.</item>
/// </list>
/// </remarks>
internal static class ApplicationInitialization
{
    /// <summary>
    /// Configures application services, including automatic discovery and DI registration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register services into.</param>
    /// <param name="logLevel">
    /// Minimum log level for service discovery diagnostics. Messages below this level will be ignored.
    /// Default is <see cref="LogLevel.Information"/>.
    /// </param>
    internal static void ConfigureApp(this IServiceCollection services,
        LogLevel logLevel = LogLevel.Information)
    {
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(logLevel);
            builder.AddProvider(new SpectreConsoleLoggerProvider(logLevel));
        });
        
        var discoveryLogger = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new SpectreConsoleLoggerProvider(logLevel));
        }).CreateLogger("ServiceDiscovery");
       
        services.AddSingleton<Camera>();
        services.AddDiscoveredServices(false, logger: discoveryLogger);
    }
}