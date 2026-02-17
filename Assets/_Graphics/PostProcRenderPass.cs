using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcRenderPass : ScriptableRenderPass
{
    private PostProcRenderFeature.Settings _settings;
    private RTHandle _source;

    // Temporary RTs
    private RTHandle _firstPassRT;
    private RTHandle _secondPassRT;

    private static readonly int ExcludedRT = Shader.PropertyToID("_ExcludedObjectsRT");

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

        // Allocate temp RTs
        RenderingUtils.ReAllocateIfNeeded(ref _firstPassRT, desc, name: "_FirstPassRT");
        RenderingUtils.ReAllocateIfNeeded(ref _secondPassRT, desc, name: "_SecondPassRT");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

    // ... rest of code

        if (_settings.paintMaterial == null || 
            _settings.ditherMaterial == null || 
            _settings.noPostProcMaterial == null) return;

        CommandBuffer cmd = CommandBufferPool.Get("PPE Passes");

        using (new ProfilingScope(cmd, profilingSampler))
        {
            // Pass 1: source → firstPassRT with paint material
            Blitter.BlitCameraTexture(cmd, _source, _firstPassRT, _settings.paintMaterial, 0);

            // Pass 2: excluded objects RT → firstPassRT with noPostProc material
            if (_settings.excludedObjectsRT != null)
            {
                cmd.SetGlobalTexture(ExcludedRT, _settings.excludedObjectsRT);
                Blitter.BlitCameraTexture(cmd, _firstPassRT, _secondPassRT, _settings.noPostProcMaterial, 0);
                // Copy result back to firstPassRT for next stage
                Blitter.BlitCameraTexture(cmd, _secondPassRT, _firstPassRT);
            }

            // Pass 3: firstPassRT → camera output with dither material
            Blitter.BlitCameraTexture(cmd, _firstPassRT, _source, _settings.ditherMaterial, 0);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }

    public void Dispose()
    {
        _firstPassRT?.Release();
        _secondPassRT?.Release();
    }
}
