using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Gpu.Buffers;

/// <summary>
/// Represents OpenGL vertex array object managing vertex input state.
/// </summary>
/// <remarks>
/// Owns vertex attribute configuration and buffer bindings used during rendering.
/// </remarks>
internal sealed class VertexArray : IDisposable
{
    /// <summary>
    /// Gets OpenGL vertex array object handle.
    /// </summary>
    private int Handle { get; } = GL.GenVertexArray();
    
    /// <summary>
    /// Binds vertex array object.
    /// </summary>
    public void Bind() =>
        GL.BindVertexArray(Handle);

    /// <summary>
    /// Unbinds a currently bound vertex array object.
    /// </summary>
    public static void Unbind() =>
        GL.BindVertexArray(0);
    
    /// <summary>
    /// Configures floating-point vertex attribute layout.
    /// </summary>
    /// <param name="index"> Vertex attribute location index. </param>
    /// <param name="componentCount"> Number of attribute components.</param>
    /// <param name="type"> Underlying attribute component type. </param>
    /// <param name="strideBytes"> Vertex stride in bytes. </param>
    /// <param name="offsetBytes"> Attribute byte offset inside vertex structure. </param>
    /// <param name="normalized"> Whether OpenGL should normalize integer values. </param>
    public void AttributePointer(
        int index,
        int componentCount,
        VertexAttribPointerType type,
        int strideBytes,
        int offsetBytes,
        bool normalized = false)
    {
        GL.VertexAttribPointer(
            index,
            componentCount,
            type,
            normalized,
            strideBytes,
            offsetBytes);

        GL.EnableVertexAttribArray(index);
    }

    /// <summary>
    /// Configures integer vertex attribute layout.
    /// </summary>
    /// <param name="index"> Vertex attribute location index. </param>
    /// <param name="componentCount"> Number of attribute components. </param>
    /// <param name="type"> Underlying integer attribute type. </param>
    /// <param name="strideBytes"> Vertex stride in bytes. </param>
    /// <param name="offsetBytes"> Attribute byte offset inside vertex structure. </param>
    public void IntegerAttributePointer(
        int index,
        int componentCount,
        VertexAttribIntegerType type,
        int strideBytes,
        int offsetBytes)
    {
        GL.VertexAttribIPointer(
            index,
            componentCount,
            type,
            strideBytes,
            offsetBytes);

        GL.EnableVertexAttribArray(index);
    }
    
    /// <summary>
    /// Binds vertex buffer for vertex attribute sourcing.
    /// </summary>
    /// <typeparam name="T"> Vertex element type. </typeparam>
    /// <param name="buffer"> Vertex buffer to bind. </param>
    public void BindVertexBuffer<T>(
        VertexBuffer<T> buffer)
        where T : unmanaged =>
        buffer.Bind();

    /// <summary>
    /// Binds index buffer for indexed rendering.
    /// </summary>
    /// <typeparam name="T"> Index element type. </typeparam>
    /// <param name="buffer"> Index buffer to bind. </param>
    public void BindIndexBuffer<T>(
        IndexBuffer<T> buffer)
        where T : unmanaged =>
        buffer.Bind();

    /// <summary>
    /// Issues non-indexed draw call.
    /// </summary>
    /// <param name="primitive"> Primitive topology to render. </param>
    /// <param name="vertexCount"> Number of vertices to render. </param>
    /// <param name="first"> Starting vertex offset. </param>
    public void DrawArrays(
        PrimitiveType primitive,
        int vertexCount,
        int first = 0) =>
        GL.DrawArrays(primitive, first, vertexCount);

    /// <summary>
    /// Issues instanced non-indexed draw call.
    /// </summary>
    /// <param name="primitive"> Primitive topology to render. </param>
    /// <param name="vertexCount"> Number of vertices per instance. </param>
    /// <param name="instanceCount"> Number of instances to render. </param>
    /// <param name="first"> Starting vertex offset. </param>
    public void DrawArraysInstanced(
        PrimitiveType primitive,
        int vertexCount,
        int instanceCount,
        int first = 0) =>
        GL.DrawArraysInstanced(primitive, first, vertexCount, instanceCount);

    /// <summary>
    /// Issues indexed draw call.
    /// </summary>
    /// <param name="primitive"> Primitive topology to render. </param>
    /// <param name="indexCount"> Number of indices to render. </param>
    /// <param name="indexType"> Underlying index element type. </param>
    /// <param name="offsetBytes"> Byte offset inside index buffer. </param>
    public void DrawElements(
        PrimitiveType primitive,
        int indexCount,
        DrawElementsType indexType,
        int offsetBytes = 0) =>
        GL.DrawElements(primitive, indexCount, indexType, offsetBytes);

    /// <summary>
    /// Issues indirect draw call using a currently bound indirect command buffer.
    /// </summary>
    /// <param name="primitive"> Primitive topology to render.</param>
    public void DrawArraysIndirect(
        PrimitiveType primitive) =>
        GL.DrawArraysIndirect(primitive, IntPtr.Zero);

    /// <summary>
    /// Configures instancing divisor for a vertex attribute.
    /// </summary>
    /// <param name="index">Vertex attribute location index.</param>
    /// <param name="divisor">Instancing divisor.</param>
    public void AttributeDivisor(
        int index,
        int divisor) =>
        GL.VertexAttribDivisor(
            index,
            divisor);
    
    public void Dispose() =>
        GL.DeleteVertexArray(Handle);
}