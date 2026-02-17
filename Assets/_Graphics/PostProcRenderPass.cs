using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcRenderPass : ScriptableRenderPass
{
    private PostProcRenderFeature.Settings _settings;
    private RTHandle _source;

    // Temporary RT for paint pass output
    private RTHandle _firstPassRT;

    public PostProcRenderPass(PostProcRenderFeature.Settings settings)
    {
        _settings = settings;
        profilingSampler = new ProfilingSampler("PPE Passes");
    }

    public void Setup(RTHandle source)
    {
        _source = source;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0; // color only

        // Only need one temp RT now
        RenderingUtils.ReAllocateIfNeeded(ref _firstPassRT, desc, name: "_FirstPassRT");
    }

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