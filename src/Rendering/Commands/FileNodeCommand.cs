using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Commands;

/// <summary>
/// Command to spawn, pulse, or remove file node on graph.
/// Renderer interprets <see cref="Action"/> to decide visual effect.
/// </summary>
/// <param name="Timestamp">Virtual time (seconds) when this command occurs.</param>
/// <param name="FilePath">Path of the file node affected.</param>
/// <param name="Action">Action to perform on the node (spawn, pulse, remove).</param>
internal sealed record FileNodeCommand(
    double Timestamp,
    string FilePath,
    FileNodeAction Action
) : RenderCommand(Timestamp);