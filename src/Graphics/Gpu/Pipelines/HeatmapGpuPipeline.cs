using ChangeTrace.Graphics.Gpu.Pipelines.Base;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Gpu.Pipelines;

/// <summary>
/// GPU pipeline for heatmap texture generation.
/// Uses compute shaders to splat object influence into a floating-point texture.
/// </summary>
internal sealed class HeatmapGpuPipeline
    : TextureComputePipeline<Vector4>
{
    /// <summary>
    /// Maximum supported heatmap source objects.
    /// </summary>
    private readonly int _maxObjects;

    /// <summary>
    /// Generated heatmap texture handle.
    /// </summary>
    public int Texture =>
        TextureHandle;

    /// <summary>
    /// Heatmap texture size in pixels.
    /// </summary>
    public Vector2i Size { get; }

    public HeatmapGpuPipeline(
        int width,
        int height,
        int maxObjects,
        ComputeShader shader)
        : base(
            Math.Max(1, width),
            Math.Max(1, height),
            CreateHeatmapTexture(
                Math.Max(1, width),
                Math.Max(1, height)),
            shader)
    {
        Size = new Vector2i(
            Math.Max(1, width),
            Math.Max(1, height));

        _maxObjects =
            Math.Max(
                1,
                maxObjects);

        InputStorageBuffer.UploadEmpty(
            _maxObjects,
            BufferUsageHint.StreamDraw);
    }

    /// <summary>
    /// Uploads heatmap source objects into GPU storage.
    /// </summary>
    public void UpdateObjectData(
        ReadOnlySpan<Vector4> objects)
    {
        if (objects.Length > _maxObjects)
        {
            throw new ArgumentException(
                "Object count exceeds maximum.",
                nameof(objects));
        }

        ItemCount = objects.Length;

        InputStorageBuffer.UploadEmpty(
            _maxObjects,
            BufferUsageHint.StreamDraw);

        if (ItemCount <= 0)
            return;

        InputStorageBuffer.SubData(objects);
    }

    /// <summary>
    /// Generates heatmap texture from uploaded object data.
    /// </summary>
    public void Generate(float maxValue = 1.0f)
    {
        Shader.Use();

        Shader.Set("objectCount", ItemCount);
        Shader.Set("maxValue", maxValue);

        BindInputBuffer();

        BindImageTexture(
            binding: 1,
            access: TextureAccess.WriteOnly,
            format: SizedInternalFormat.Rgba32f);

        Dispatch2D(
            localSizeX: 16,
            localSizeY: 16);

        Barrier();

        UnbindInputBuffer();

        UnbindImageTexture(
            binding: 1,
            access: TextureAccess.WriteOnly,
            format: SizedInternalFormat.Rgba32f);
    }

    /// <summary>
    /// Creates floating-point heatmap texture storage.
    /// </summary>
    private static int CreateHeatmapTexture(
        int width,
        int height)
    {
        int texture =
            GL.GenTexture();

        GL.BindTexture(
            TextureTarget.Texture2D,
            texture);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba32f,
            width,
            height,
            0,
            PixelFormat.Rgba,
            PixelType.Float,
            IntPtr.Zero);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);

        GL.BindTexture(
            TextureTarget.Texture2D,
            0);

        return texture;
    }
}