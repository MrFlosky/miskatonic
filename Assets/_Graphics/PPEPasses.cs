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
    public Material noPostProcMaterial;
    public RenderTexture firstPassRT;   // Results from paint effect

    
    /// <summary>
    /// Way this works is by having camera that only renders noPostProc layer with transparent clear color, store
    /// in render texture. Then blit this rtex atop first paint render pass.
    /// </summary>
    public RenderTexture excludedObjectsRT; // render texture containing camera view of only excluded post proc layer objects
    public Camera excludedCam;

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
            Graphics.Blit(source, firstPassRT, paintMaterial); // store results from source image blit with paint material in firstPassRt
            Graphics.Blit(excludedObjectsRT, firstPassRT, noPostProcMaterial); // store exluded objects rt pixels with transparent material in first pass rt
            Graphics.Blit(firstPassRT, destination, ditherMaterial); // store firstpassrt with dither material into destination, output render texture in inspector
            
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

        void LateUpdate()
    {
        // Sync excluded camera with main camera (after Cinemachine updates)
  
            // Sync transform (position and rotation)
            excludedCam.transform.position = Camera.main.transform.position;
            excludedCam.transform.rotation = Camera.main.transform.rotation;
            
            // Sync camera properties
            excludedCam.fieldOfView = Camera.main.fieldOfView;
            excludedCam.nearClipPlane = Camera.main.nearClipPlane;
            excludedCam.farClipPlane = Camera.main.farClipPlane;
            excludedCam.orthographic = Camera.main.orthographic;
            excludedCam.orthographicSize = Camera.main.orthographicSize;
        
    }

    void OnDestroy()
    {
        if (firstPassRT != null) firstPassRT.Release();
    }
}
