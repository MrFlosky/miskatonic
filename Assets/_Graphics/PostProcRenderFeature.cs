using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material paintMaterial;
        public Material ditherMaterial;
        public Material noPostProcMaterial;
        public RenderTexture excludedObjectsRT;
    }

    public Settings settings = new Settings();
    private PostProcRenderPass _pass;

    public override void Create()
    {
        _pass = new PostProcRenderPass(settings);
        _pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // STEP 1: Just enqueue the pass here, don't call Setup yet
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_pass);
    }

    // STEP 2: Call Setup here instead — cameraColorTargetHandle is ready at this point
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game) return;
        _pass.Setup(renderer.cameraColorTargetHandle);
    }
}