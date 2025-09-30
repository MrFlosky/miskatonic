using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class in charge of performing two render passes. 
/// One from Unity's auto-generated render texture containing the raw scene,
/// through the paint pass. Then the paint render texture through the dither pass.
/// </summary>
public class PPEPasses : MonoBehaviour
{

    public Material paintMaterial;
    public Material ditherMaterial;
    public RenderTexture firstPassRT;   // Results from paint effect
    //public RenderTexture secondPassRT;

    /// <summary>
    /// Runs once per frame when camera renders automatically.
    /// </summary>
    /// <param name="source">
    /// rendered scene before any post proc, automatically created by unity
    /// </param>
    /// <param name="destination">
    /// final output, render texture target on camera 
    /// </param>
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (ppEnabled)
        {
            // perform two render passes using first paint shader, then dither
            Graphics.Blit(source, firstPassRT, paintMaterial);
            Graphics.Blit(firstPassRT, destination, ditherMaterial);
        }
        else
        {
            // raw scene
            Graphics.Blit(source, destination);
        }
    }

    bool ppEnabled = true;
    void Update()
    {
        // Toggle effects with P 
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            ppEnabled = !ppEnabled;
        }
    }

    void OnDestroy()
    {
        if (firstPassRT != null) firstPassRT.Release();
    }
}
