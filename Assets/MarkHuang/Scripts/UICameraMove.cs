using UnityEngine;
using UnityEngine.InputSystem;

public class UICameraMove : MonoBehaviour
{
    public float moveAmount = 0.5f;  // Max displacement of camera
    public float smoothSpeed = 5f;    // Smooth camera movement
    private Vector3 initialPosition;  // Initial position of camera
    private Vector3 targetPosition;   // Target position of camera

    private Vector2 mousePosition;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions(); // Initialize Input Actions
    }

    private void OnEnable()
    {
        inputActions.Player.MousePosition.performed += ctx => OnMouseMove(ctx.ReadValue<Vector2>());
        inputActions.Enable(); // Enable Input Actions
    }

    private void OnDisable()
    {
        inputActions.Disable(); // Disable Input Actions
    }

    void Start()
    {
        // Record the initial position of the camera
        initialPosition = transform.position;
        targetPosition = initialPosition;
    }

    void Update()
    {
        // Calculate target position of camera, limited by max displacement
        float mouseX = (mousePosition.x / Screen.width) - 0.5f;
        float mouseY = (mousePosition.y / Screen.height) - 0.5f;

        Vector3 offset = new Vector3(mouseX * moveAmount, mouseY * moveAmount, 0);
        targetPosition = initialPosition + offset;

        // Use Lerp to make camera movement smooth
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    private void OnMouseMove(Vector2 position)
    {
        mousePosition = position;
    }
}
