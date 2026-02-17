using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ChangeTrace.Cli.Logging;

/// <summary>
/// Custom logger that writes log messages to the console using <see cref="Spectre.Console"/>.
/// Designed to work alongside spinners without breaking the console output.
/// </summary>
/// <param name="categoryName">The category name associated with this logger (typically the class name).</param>
/// <param name="minLevel">The minimum <see cref="LogLevel"/> that will be logged. Messages below this level are ignored.</param>
internal sealed class SpectreConsoleLogger(string categoryName, LogLevel minLevel) : ILogger
{
    /// <summary>
    /// Begins logical operation scope. This implementation does not support scopes.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state">The state to associate with the scope.</param>
    /// <returns>Always <c>null</c> because scopes are not supported.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <summary>
    /// Determines whether the given <paramref name="logLevel"/> is enabled.
    /// </summary>
    /// <param name="logLevel">The level to check.</param>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= minLevel;

    /// <summary>
    /// Writes a log entry to the console.
    /// </summary>
    /// <typeparam name="TState">The type of the object to be logged.</typeparam>
    /// <param name="logLevel">Level of the log message.</param>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="state">The object to log.</param>
    /// <param name="exception">Optional exception related to the log entry.</param>
    /// <param name="formatter">Function to convert the state and exception to a message string.</param>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var shortCategory = categoryName.Split('.').LastOrDefault() ?? categoryName;

        var (color, level) = logLevel switch
        {
            LogLevel.Trace => ("grey", "TRACE"),
            LogLevel.Debug => ("grey", "DEBUG"),
            LogLevel.Information => ("white", "INFO"),
            LogLevel.Warning => ("yellow", "WARN"),
            LogLevel.Error => ("red", "ERROR"),
            LogLevel.Critical => ("red bold", "FATAL"),
            _ => ("white", "INFO")
        };

        var markup = $"[grey]{DateTime.Now:HH:mm:ss}[/] [{color}]{level,-5}[/] [grey]{shortCategory}:[/] {Markup.Escape(message)}";
        AnsiConsole.MarkupLine(markup);

        if (exception != null)
        {
            AnsiConsole.WriteException(exception, ExceptionFormats.ShortenPaths);
        }
    }
}
