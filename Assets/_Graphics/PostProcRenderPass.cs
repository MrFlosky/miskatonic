using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Actual post post proc pass for paint and dither. Records and submits GPU
/// command for blitting shaders over camera's render texture.
/// </summary>
public class PostProcRenderPass : ScriptableRenderPass
{
    private PostProcRenderFeature.Settings _settings;
    private RTHandle _source;

    // Temporary RT for paint pass output
    private RTHandle _firstPassRT;

    /// <summary>
    /// Init render pass with settings (materials we want to blit over our color texture attachment)
    /// </summary>
    /// <param name="settings"></param>
    public PostProcRenderPass(PostProcRenderFeature.Settings settings)
    {
        _settings = settings;
        profilingSampler = new ProfilingSampler("PPE Passes"); // debug for profiling
    }

    /// <summary>
    /// Called by feature every frame to store updated color texture attachment.
    /// </summary>
    /// <param name="source"></param>
    public void Setup(RTHandle source)
    {
        _source = source;
    }

    /// <summary>
    /// Init/reuse temp render texture with camera's params, res, and only color attachment.
    /// Runs before execute each frame.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="renderingData"></param>
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0; // color only
        RenderingUtils.ReAllocateIfNeeded(ref _firstPassRT, desc, name: "_FirstPassRT");
    }

    /// <summary>
    /// Record GPU commands for blitting then submit them!
    /// </summary>
    /// <param name="context"></param>
    /// <param name="renderingData"></param>
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_source == null) { Debug.LogError("PPE: _source is null"); return; }
        if (_firstPassRT == null) { Debug.LogError("PPE: _firstPassRT is null"); return; }
        if (_settings.paintMaterial == null) { Debug.LogError("PPE: paintMaterial not assigned"); return; }
        if (_settings.ditherMaterial == null) { Debug.LogError("PPE: ditherMaterial not assigned"); return; }


        CommandBuffer cmd = CommandBufferPool.Get("PPE Passes");
        using (new ProfilingScope(cmd, profilingSampler))
        {
            // Pass 1: source → firstPassRT with paint material
            Blitter.BlitCameraTexture(cmd, _source, _firstPassRT, _settings.paintMaterial, 0);

            // Pass 2: firstPassRT → camera output with dither material
            Blitter.BlitCameraTexture(cmd, _firstPassRT, _source, _settings.ditherMaterial, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }

    public void Dispose()
    {
        _firstPassRT?.Release();
    }
}