using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors;

/// <summary>
/// Dispatches <see cref="RenderCommand"/> instances to appropriate <see cref="IRenderCommandHandler"/>.
/// </summary>
/// <remarks>
/// Handlers are registered by their <see cref="IRenderCommandHandler.CommandType"/> and cached
/// in lookup dictionary. When command is dispatched, matching handler processes it
/// with the current virtual time.
/// </remarks>
internal sealed class SceneCommandDispatcher
{
    private readonly IReadOnlyDictionary<Type, IRenderCommandHandler> _handlers;

    /// <summary>
    /// Initializes new dispatcher with given collection of handlers.
    /// </summary>
    /// <param name="handlers">Collection of render command handlers to register.</param>
    public SceneCommandDispatcher(IEnumerable<IRenderCommandHandler> handlers) =>
        _handlers = handlers.ToDictionary(h => h.CommandType);

    /// <summary>
    /// Dispatches <see cref="RenderCommand"/> to corresponding handler, if one exists.
    /// </summary>
    /// <param name="command">The render command to dispatch.</param>
    /// <param name="virtualTime">Current virtual timeline time (seconds).</param>
    public void Dispatch(RenderCommand command, double virtualTime)
    {
        if (_handlers.TryGetValue(command.GetType(), out var handler))
        {
            handler.Handle(command, virtualTime);
        }
    }
}