using ChangeTrace.Graphics.Gpu.Contracts;
using ChangeTrace.Graphics.Gpu.Pipelines.Base;
using ChangeTrace.Graphics.Shaders.Runtime;
using ChangeTrace.Rendering.Snapshots;
using OpenTK.Mathematics;
using GpuVector2 = System.Numerics.Vector2;
using GpuVector3 = System.Numerics.Vector3;
using GpuVector4 = System.Numerics.Vector4;

namespace ChangeTrace.Graphics.Gpu.Pipelines;

/// <summary>
/// GPU pipeline for pawn rendering.
/// Handles pawn upload, visibility compazction and indirect rendering.
/// </summary>
internal sealed class PawnGpuPipeline
    : IndirectComputePipeline<GpuPawn>
{
    /// <summary>
    /// GPU pawn upload capacity.
    /// </summary>
    private const int MaxPawns = 128;

    /// <summary>
    /// Minimum allowed zoom factor.
    /// Prevents division instability.
    /// </summary>
    private const float MinZoom = 0.01f;

    /// <summary>
    /// Minimum visible pawn radius in screen space.
    /// </summary>
    private const float MinScreenRadius = 6f;

    private readonly GpuPawn[] _cpuUpload =
        new GpuPawn[MaxPawns];

    /// <summary>
    /// Visible compacted pawn buffer handle.
    /// </summary>
    public int VisiblePawnBuffer =>
        VisibleStorageBuffer.Handle;

    /// <summary>
    /// Indirect draw command buffer handle.
    /// </summary>
    public int IndirectBuffer =>
        IndirectDrawBuffer.Handle;

    public PawnGpuPipeline()
    {
        Initialize(
            maxItems: MaxPawns,
            vertexCountPerInstance: 6);
    }

    /// <summary>
    /// Uploads pawns into GPU storage.
    /// </summary>
    public int UploadPawns(
        IEnumerable<AvatarSnapshot> avatarsEnum,
        float zoom,
        float minAlpha = 0.01f)
    {
        IReadOnlyList<AvatarSnapshot> avatars =
            avatarsEnum as IReadOnlyList<AvatarSnapshot>
            ?? avatarsEnum.ToList();

        int count = 0;

        float safeZoom =
            MathF.Max(
                zoom,
                MinZoom);

        for (int i = 0;
             i < avatars.Count && count < MaxPawns;
             i++)
        {
            AvatarSnapshot avatar = avatars[i];

            if (avatar.Alpha < minAlpha)
                continue;

            float activity =
                MathF.Min(
                    avatar.ActivityLevel,
                    1.5f);

            float baseRadius =
                8f + 4f * activity;

            float radius =
                baseRadius * safeZoom < MinScreenRadius
                    ? MinScreenRadius / safeZoom
                    : baseRadius;

            float alpha =
                avatar.Color.W *
                avatar.Alpha *
                MathF.Max(
                    0.4f,
                    avatar.ActivityLevel);

            _cpuUpload[count++] = new GpuPawn
            {
                Position = new GpuVector2(
                    avatar.Position.X,
                    avatar.Position.Y),

                Radius = radius,

                Alpha = alpha,

                Color = new GpuVector4(
                    avatar.Color.X,
                    avatar.Color.Y,
                    avatar.Color.Z,
                    avatar.Color.W),

                Glow = avatar.ActivityLevel,

                Padding = GpuVector3.Zero
            };
        }

        if (count <= 0)
            return 0;

        InputStorageBuffer.SubData(
            _cpuUpload.AsSpan(0, count));

        return count;
    }

    /// <summary>
    /// Executes compute visibility pass for uploaded pawns.
    /// </summary>
    public void Execute(
        ComputeShader computeShader,
        int pawnCount,
        Matrix3 viewProj,
        Vector2 viewport,
        float zoom)
    {
        if (pawnCount <= 0)
            return;

        ResetIndirect();

        computeShader.Use();
        computeShader.Set("uPawnCount", pawnCount);
        computeShader.Set("uViewProj", viewProj);
        computeShader.Set("uViewport", viewport);
        computeShader.Set("uZoom", zoom);

        BindCommonBuffers();
        Dispatch256(pawnCount);
    }
}