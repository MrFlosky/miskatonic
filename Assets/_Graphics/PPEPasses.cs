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
    public Camera excludedCam;
    
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

}
