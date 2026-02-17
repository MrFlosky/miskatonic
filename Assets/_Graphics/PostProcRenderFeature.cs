using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Render feature to setup and run post processing pass for paint and dither.
/// </summary>
public class PostProcRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material paintMaterial;
        public Material ditherMaterial;
    }

    public Settings settings = new Settings();
    private PostProcRenderPass _pass;

    /// <summary>
    /// Create pass.
    /// </summary>
    public override void Create()
    {
        _pass = new PostProcRenderPass(settings);
        // perform pass after all geometry is drawn but before urp's own post proc
        _pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    /// <summary>
    /// Run every frame for every camera.
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="renderingData"></param>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // only apply effect for game camera, not scene or others
        if (renderingData.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_pass);
    }

    /// <summary>
    /// Run every frame for every camera after AddRenderPasses.
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="renderingData"></param>
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game) return;
        // setup render pass with current color attachment
        _pass.Setup(renderer.cameraColorTargetHandle);
    }
}