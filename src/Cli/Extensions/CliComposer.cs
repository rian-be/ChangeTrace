using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Extensions;

/// <summary>
/// Composes the CLI command tree from registered <see cref="ICliCommand"/> definitions.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Discovers all <see cref="ICliCommand"/> implementations from the DI container.</item>
/// <item>Builds <see cref="Command"/> instances and attaches handlers where defined.</item>
/// <item>Constructs a hierarchical command tree based on parent relationships.</item>
/// <item>Returns a fully configured <see cref="RootCommand"/> ready for execution.</item>
/// <item>Provides a diagnostic utility to print the composed command structure.</item>
/// </list>
/// </remarks>
internal static class CliComposer
{
    /// <summary>
    /// Builds the complete <see cref="RootCommand"/> using registered CLI command definitions.
    /// </summary>
    /// <param name="provider">Service provider used to resolve command definitions and handlers.</param>
    /// <returns>A fully composed <see cref="RootCommand"/>.</returns>
    public static RootCommand Build(IServiceProvider provider)
    {
        var definitions = provider.GetServices<ICliCommand>().ToList();
        var commandMap = BuildCommands(provider, definitions);

        return ComposeTree(definitions, commandMap);
    }

    /// <summary>
    /// Instantiates all command definitions and attaches their handlers.
    /// </summary>
    /// <param name="provider">Service provider used to resolve handlers.</param>
    /// <param name="definitions">Collection of CLI command definitions.</param>
    /// <returns>
    /// A dictionary mapping command definition types to constructed <see cref="Command"/> instances.
    /// </returns>
    private static Dictionary<Type, Command> BuildCommands(
        IServiceProvider provider,
        IEnumerable<ICliCommand> definitions)
    {
        var map = new Dictionary<Type, Command>();

        foreach (var def in definitions)
        {
            var cmd = def.Build();

            if (def.HandlerType != null)
                cmd.AttachHandler(provider, def.HandlerType);

            map[def.GetType()] = cmd;
        }

        return map;
    }

    /// <summary>
    /// Composes the hierarchical command tree based on parent-child relationships.
    /// </summary>
    /// <param name="definitions">Command definitions describing the hierarchy.</param>
    /// <param name="map">Previously built command instances.</param>
    /// <returns>The root command containing the full CLI structure.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a declared parent command cannot be found.
    /// </exception>
    private static RootCommand ComposeTree(
        IEnumerable<ICliCommand> definitions,
        Dictionary<Type, Command> map)
    {
        var root = new RootCommand("ChangeTrace - repository timeline generator");

        foreach (var def in definitions)
        {
            var cmd = map[def.GetType()];

            if (def.Parent == null)
            {
                root.Subcommands.Add(cmd);
                continue;
            }

            if (!map.TryGetValue(def.Parent, out var parent))
                throw new InvalidOperationException(
                    $"Parent '{def.Parent.Name}' not found for '{def.GetType().Name}'");

            parent.Subcommands.Add(cmd);
        }

        return root;
    }

    /// <summary>
    /// Recursively prints the CLI command tree for diagnostic purposes.
    /// </summary>
    /// <param name="cmd">The command to print.</param>
    /// <param name="indent">Indentation prefix for nested commands.</param>
    public static void Dump(Command cmd, string indent = "")
    {
        Console.WriteLine($"{indent}{cmd.Name}");

        foreach (var child in cmd.Subcommands.OrderBy(c => c.Name))
            Dump(child, indent + "  ");
    }
}
