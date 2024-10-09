using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera movement & zoom in/out
/// </summary>
public class CameraControl : MonoBehaviour
{
    [Header("Camera Settings")]
    public float moveSpeed = 5f;       // camera move speed
    public float zoomSpeed = 10f;      // camera zoom speed
    public float edgeThreshold = 10f;  // the Threshold of the edge(begin to move)

    private float screenWidth;
    private float screenHeight;

    private Transform cameraEntity;

    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        cameraEntity = transform.GetChild(0);  //Main Camera always the first child of the CameraHolder
    }

    void Update()
    {
        CameraMove();
        CameraZoom();
    }

    /// <summary>
    /// Move Camera horizontal / vertical
    /// </summary>
    void CameraMove()
    {
        Vector3 cameraMove = Vector3.zero;

        // get mouse position
        Vector3 mousePosition = Input.mousePosition;

        // Check if the left edge of the screen is touched
        if (mousePosition.x <= edgeThreshold)
        {
            cameraMove.x = -1f;
        }

        else if (mousePosition.x >= screenWidth - edgeThreshold)
        {
            cameraMove.x = 1f;
        }

        if (mousePosition.y <= edgeThreshold)
        {
            cameraMove.z = -1f;
        }

        else if (mousePosition.y >= screenHeight - edgeThreshold)
        {
            cameraMove.z = 1f;
        }

        // move the camera
        transform.Translate(cameraMove * moveSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Zoom Camera
    /// </summary>
    void CameraZoom()
    {
        // get mouse scroll Input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // if mouse scroll gose up£¬scrollInput > 0£»else£¬scrollInput < 0
        if (scrollInput != 0)
        {
            // zoom camera
            cameraEntity.Translate(Vector3.forward * scrollInput * zoomSpeed * Time.deltaTime);
        }
    }
}
