using UnityEngine;

public class UICameraMove : MonoBehaviour
{
    public float moveAmount = 0.5f;  // max displacement of camera
    public float smoothSpeed = 5f;   // Parameters for smooth camera movement
    private Vector3 initialPosition; // the initial pos of camera
    private Vector3 targetPosition;  // the target pos of camera

    void Start()
    {
        // record the initial pos of camera
        initialPosition = transform.position;
        targetPosition = initialPosition;
    }

    void Update()
    {
        // get mouse pos and displacement
        Vector2 mousePosition = Input.mousePosition;
        float mouseX = (mousePosition.x / Screen.width) - 0.5f;
        float mouseY = (mousePosition.y / Screen.height) - 0.5f;

        // calculate target pos of camera, limited by max displacement of camera
        Vector3 offset = new Vector3(mouseX * moveAmount, mouseY * moveAmount, 0);
        targetPosition = initialPosition + offset;

        // Use Lerp function to make it smooth
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
