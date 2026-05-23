using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Translators;

/// <summary>
/// Coordinates event translators responsible for converting
/// domain events into render commands.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class TranslationPipeline : ITranslationPipeline
{
    private readonly List<(int Priority, IEventTranslator Translator)> _translators = [];

    /// <summary>
    /// Creates the default translator configuration used by the renderer.
    /// </summary>
    public static TranslationPipeline Default()
    {
        var pipeline = new TranslationPipeline();

        pipeline.Register(new CommitBundleTranslator(), priority: 5);
        // pipeline.Register(new BranchTranslator(), priority: 10);

        // pipeline.Register(new CommitTranslator(), priority: 10);
        // pipeline.Register(new BranchTranslator(), priority: 10);
        // pipeline.Register(new PullRequestTranslator(), priority: 10);

        return pipeline;
    }

    /// <summary>
    /// Registers translator with execution priority.
    /// Lower values execute earlier.
    /// </summary>
    public void Register(
        IEventTranslator translator,
        int priority = 10)
    {
        _translators.Add((priority, translator));

        _translators.Sort(
            static (a, b) =>
                a.Priority.CompareTo(b.Priority));
    }

    /// <summary>
    /// Translates event into a render command sequence.
    /// </summary>
    public IReadOnlyList<RenderCommand> Translate(object evt)
    {
        var commands = new List<RenderCommand>();

        foreach (var (_, translator) in _translators)
        {
            if (translator.EventType != evt.GetType())
                continue;

            if (!translator.CanHandle(evt))
                continue;

            commands.AddRange(
                translator.Translate(evt));
        }

        return commands;
    }
}