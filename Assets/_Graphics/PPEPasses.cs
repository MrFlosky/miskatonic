using UnityEngine;

/// <summary>
/// This class is only responsible for making the overlay camera follow
/// the cinemachine controlled main camera. RenderFeature on URP Asset handles
/// the post processing.
/// </summary>
public class PPEPasses : MonoBehaviour
{   
    /// <summary>
    /// Overlay camera that ONLY renders non-post proc layer.
    /// </summary>
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
