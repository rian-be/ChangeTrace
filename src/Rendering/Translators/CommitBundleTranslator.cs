using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Translators;

internal sealed class CommitBundleTranslator : IEventTranslator
{
    public Type EventType => typeof(CommitBundleEvent);

    public bool CanHandle(object evt) => evt is CommitBundleEvent;

    public IEnumerable<RenderCommand> Translate(object evt)
    {
        var e = (CommitBundleEvent)evt;
        var actor = ActorName.Create(e.Actor).Value;
        var files = e.Files.ToArray();

        // DEBUG
       //Console.WriteLine($"[CommitBundleTranslator] Commit: {e.CommitSha}");
       // Console.WriteLine($"Actor: {actor}");
        //Console.WriteLine($"Files ({files.Length}):");

        //foreach (var f in files) Console.WriteLine($"  - {f}");

        bool firstFile = true;
        foreach (var file in files)
        {
            if (firstFile)
            {
                yield return new MoveActorCommand(e.Timestamp, actor, file, false, e.CommitSha);
                firstFile = false;
            }

            yield return new FileNodeCommand(e.Timestamp, file, FileNodeAction.Pulse, e.Actor, e.CommitSha);
            
            var color = ColorPalette.ForFilePath(file);
            yield return new ParticleBurstCommand(e.Timestamp, file, 12, ColorPalette.Vec4ToUInt(color));
        }

        if (files.Length > 0)
        {
           // Console.WriteLine($"BundledEdge -> {files.Length} targets");
            yield return new BundledEdgeCommand(actor, files, EdgeKind.Commit, e.Timestamp);
        }
       // Console.WriteLine($"FRAME {DateTime.Now.Ticks}");
    }
}