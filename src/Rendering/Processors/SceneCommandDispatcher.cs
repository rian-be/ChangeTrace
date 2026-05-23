using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors;

/// <summary>
/// Dispatches render commands to matching handlers.
/// </summary>
internal sealed class SceneCommandDispatcher(
    IEnumerable<IRenderCommandHandler> handlers)
{
    private readonly IReadOnlyDictionary<Type, IRenderCommandHandler> _handlers =
        handlers.ToDictionary(
            h => h.CommandType);

    /// <summary>
    /// Dispatches render the command to the appropriate handler.
    /// </summary>
    public void Dispatch(
        RenderCommand command,
        double virtualTime)
    {
        if (_handlers.TryGetValue(
                command.GetType(),
                out IRenderCommandHandler? handler))
        {
            handler.Handle(
                command,
                virtualTime);
        }
    }
}