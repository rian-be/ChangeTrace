namespace ChangeTrace.Rendering.Commands;

/// <summary>
/// Base record for all render commands in the rendering pipeline.
/// Renderer interprets these to draw visuals; player does not handle pixels.
/// </summary>
/// <param name="Timestamp">Virtual time (seconds) when this command should occur.</param>
internal abstract record RenderCommand(double Timestamp);