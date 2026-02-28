using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Processors;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Defines handler that processes specific type of <see cref="RenderCommand"/>.
/// </summary>
/// <remarks>
/// Implementations are responsible for updating the scene graph or triggering animations
/// in response to commands dispatched by <see cref="SceneCommandDispatcher"/>.
/// </remarks>
internal interface IRenderCommandHandler
{
    /// <summary>
    /// Gets the type of <see cref="RenderCommand"/> this handler can process.
    /// </summary>
    Type CommandType { get; }

    /// <summary>
    /// Handles  specified <see cref="RenderCommand"/>.
    /// </summary>
    /// <param name="command">The command to process.</param>
    /// <param name="virtualTime">The current logical time in simulation.</param>
    void Handle(RenderCommand command, double virtualTime);
}