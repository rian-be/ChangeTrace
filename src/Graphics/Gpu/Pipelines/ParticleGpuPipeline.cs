using ChangeTrace.Graphics.Gpu.Contracts;
using ChangeTrace.Graphics.Gpu.Pipelines.Base;
using ChangeTrace.Graphics.Shaders.Runtime;
using ChangeTrace.Rendering.Snapshots;
using OpenTK.Mathematics;
using GpuVector2 = System.Numerics.Vector2;
using GpuVector4 = System.Numerics.Vector4;

namespace ChangeTrace.Graphics.Gpu.Pipelines;

/// <summary>
/// GPU pipeline for particle rendering.
/// Handles particle upload, visibility compaction and indirect rendering.
/// </summary>
internal sealed class ParticleGpuPipeline
    : IndirectComputePipeline<GpuParticle>
{
    /// <summary>
    /// GPU particle upload capacity.
    /// </summary>
    private const int MaxParticles = 8192;

    private readonly GpuParticle[] _cpuUpload =
        new GpuParticle[MaxParticles];

    /// <summary>
    /// Visible compacted particle buffer handle.
    /// </summary>
    public int VisibleParticleBuffer =>
        VisibleStorageBuffer.Handle;

    /// <summary>
    /// Indirect draw command buffer handle.
    /// </summary>
    public int IndirectBuffer =>
        IndirectDrawBuffer.Handle;

    public ParticleGpuPipeline()
    {
        Initialize(
            maxItems: MaxParticles,
            vertexCountPerInstance: 1);
    }

    /// <summary>
    /// Uploads particles into GPU storage.
    /// </summary>
    public int UploadParticles(
        IReadOnlyList<ParticleSnapshot> particles)
    {
        int count =
            Math.Min(
                particles.Count,
                MaxParticles);

        if (count <= 0)
            return 0;

        for (int i = 0; i < count; i++)
        {
            ParticleSnapshot particle = particles[i];

            _cpuUpload[i] = new GpuParticle
            {
                Position = new GpuVector2(
                    particle.Position.X,
                    particle.Position.Y),

                Size = particle.Size,

                Alpha = particle.Alpha,

                Color = new GpuVector4(
                    particle.Color.X,
                    particle.Color.Y,
                    particle.Color.Z,
                    particle.Color.W)
            };
        }

        InputStorageBuffer.SubData(
            _cpuUpload.AsSpan(0, count));

        return count;
    }

    /// <summary>
    /// Executes compute visibility pass for uploaded particles.
    /// </summary>
    public void Execute(
        ComputeShader computeShader,
        int particleCount,
        Matrix3 viewProj,
        Vector2 viewport,
        float zoom)
    {
        if (particleCount <= 0)
            return;

        ResetIndirect();

        computeShader.Use();
        computeShader.Set("uParticleCount", particleCount);
        computeShader.Set("uViewProj", viewProj);
        computeShader.Set("uViewport", viewport);
        computeShader.Set("uZoom", zoom);

        BindCommonBuffers();
        Dispatch256(particleCount);
    }
}