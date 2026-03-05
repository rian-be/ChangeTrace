using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.OpenTK.Renderer;

internal sealed class EdgeRenderer : IDisposable
{
    private const int MaxEdges      = 2048;
    private const int VertsPerEdge  = 4;
    private const int FloatsPerVert = 11; // from.xy to.xy color.rgba alpha width corner

    private readonly int _vao;
    private readonly GpuBuffer<float> _vbo;
    private readonly GpuBuffer<int>   _ebo;

    private readonly float[] _vertData = new float[MaxEdges * VertsPerEdge * FloatsPerVert];
    private readonly int[]   _indices;

    internal EdgeRenderer()
    {
        _indices = new int[MaxEdges * 6];
        for (int i = 0; i < MaxEdges; i++)
        {
            int v = i * 4;
            _indices[i * 6 + 0] = v;
            _indices[i * 6 + 1] = v + 1;
            _indices[i * 6 + 2] = v + 2;
            _indices[i * 6 + 3] = v + 1;
            _indices[i * 6 + 4] = v + 3;
            _indices[i * 6 + 5] = v + 2;
        }

        _vao = GL.GenVertexArray();
        _vbo = new GpuBuffer<float>(BufferTarget.ArrayBuffer);
        _ebo = new GpuBuffer<int>(BufferTarget.ElementArrayBuffer);

        GL.BindVertexArray(_vao);
        _vbo.UploadEmpty(MaxEdges * VertsPerEdge * FloatsPerVert);
        _ebo.Upload(_indices.AsSpan(), BufferUsageHint.StaticDraw);

        int stride = FloatsPerVert * sizeof(float);
        SetAttrib(0, 2, stride, 0);                // aFrom
        SetAttrib(1, 2, stride, 2 * sizeof(float)); // aTo
        SetAttrib(2, 4, stride, 4 * sizeof(float)); // aColor
        SetAttrib(3, 1, stride, 8 * sizeof(float)); // aAlpha
        SetAttrib(4, 1, stride, 9 * sizeof(float)); // aWidth
        SetAttrib(5, 1, stride, 10 * sizeof(float));// aCorner
        GL.BindVertexArray(0);
    }

    internal void Draw(IReadOnlyList<EdgeSnapshot> edges, ISceneSnapshot scene,
                       Shaders.ShaderProgram shader, Matrix3 viewProj)
    {
        int count = Math.Min(edges.Count, MaxEdges);
        if (count == 0) return;

        int ptr = 0;
        int drawn = 0;
        foreach (var edge in edges)
        {
            if (drawn >= MaxEdges) break;

            var from = scene.FindNode(edge.FromId);
            var to   = scene.FindNode(edge.ToId);
            if (from == null || to == null) continue;

            var col = edge.Color;
            float width = edge.Kind == Rendering.Enums.EdgeKind.Merge ? 2.0f : 1.0f;

            for (int c = 0; c < 4; c++)
            {
                _vertData[ptr++] = from.Position.X;
                _vertData[ptr++] = from.Position.Y;
                _vertData[ptr++] = to.Position.X;
                _vertData[ptr++] = to.Position.Y;
                _vertData[ptr++] = col.X;
                _vertData[ptr++] = col.Y;
                _vertData[ptr++] = col.Z;
                _vertData[ptr++] = col.W;
                _vertData[ptr++] = edge.Alpha;
                _vertData[ptr++] = width;
                _vertData[ptr++] = c;
            }
            drawn++;
        }

        _vbo.Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, nint.Zero, ptr * sizeof(float), _vertData);

        shader.Use();
        shader.Set("uViewProj", viewProj);

        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, drawn * 6, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
    
    /// <summary>
    /// Draws edges using a mix of scene nodes AND a list of virtual (synthetic) node snapshots.
    /// Used by PerspectiveTimelineRenderer for road markers that have no real scene nodes.
    /// </summary>
    internal void DrawWithVirtualNodes(
        IReadOnlyList<EdgeSnapshot> edges,
        IEnumerable<NodeSnapshot> virtualNodes,
        Shaders.ShaderProgram shader,
        Matrix3 viewProj)
    {
        // Build lookup: id → position from virtual nodes
        var lookup = virtualNodes.ToDictionary(n => n.Id, n => n.Position);

        int count = Math.Min(edges.Count, MaxEdges);
        if (count == 0) return;

        int ptr = 0, drawn = 0;
        foreach (var edge in edges)
        {
            if (drawn >= MaxEdges) break;

            Vec2? fromPos = null, toPos = null;

            if (lookup.TryGetValue(edge.FromId, out var fp)) fromPos = fp;
            if (lookup.TryGetValue(edge.ToId,   out var tp)) toPos   = tp;

            if (fromPos == null || toPos == null) continue;

            var col = edge.Color;
            float width = 0.8f;

            for (int c = 0; c < 4; c++)
            {
                _vertData[ptr++] = fromPos.Value.X;
                _vertData[ptr++] = fromPos.Value.Y;
                _vertData[ptr++] = toPos.Value.X;
                _vertData[ptr++] = toPos.Value.Y;
                _vertData[ptr++] = col.X;
                _vertData[ptr++] = col.Y;
                _vertData[ptr++] = col.Z;
                _vertData[ptr++] = col.W;
                _vertData[ptr++] = edge.Alpha;
                _vertData[ptr++] = width;
                _vertData[ptr++] = c;
            }
            drawn++;
        }

        _vbo.Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, nint.Zero, ptr * sizeof(float), _vertData);
        shader.Use();
        shader.Set("uViewProj", viewProj);
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, drawn * 6, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }
    
    private static void SetAttrib(int index, int size, int stride, int offset)
    {
        GL.EnableVertexAttribArray(index);
        GL.VertexAttribPointer(index, size, VertexAttribPointerType.Float, false, stride, offset);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_vao);
        _vbo.Dispose();
        _ebo.Dispose();
    }
}