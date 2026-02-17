using Microsoft.Extensions.Logging;

namespace ChangeTrace.Cli.Logging;

/// <summary>
/// Provides logger implementation that writes logs to the console using Spectre.Console.
/// </summary>
/// <remarks>
/// This logger provider creates instances of <see cref="SpectreConsoleLogger"/> and supports filtering
/// by <see cref="LogLevel"/>. Use this provider to integrate structured console logging in CLI applications.
/// </remarks>
/// <param name="minLevel">Minimum <see cref="LogLevel"/> to log. Messages below this level are ignored.</param>
internal sealed class SpectreConsoleLoggerProvider(LogLevel minLevel) : ILoggerProvider
{
    /// <summary>
    /// Creates a <see cref="SpectreConsoleLogger"/> for the specified category.
    /// </summary>
    /// <param name="categoryName">Category name for the logger.</param>
    /// <returns>An <see cref="ILogger"/> instance writing to the console via Spectre.Console.</returns>
    public ILogger CreateLogger(string categoryName) => new SpectreConsoleLogger(categoryName, minLevel);

    /// <summary>
    /// Performs any necessary cleanup. This implementation does nothing.
    /// </summary>
    public void Dispose() { }
}