using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float panBorderThickness = 10f;
    [SerializeField] private Vector2 panLimitMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 panLimitMax = new Vector2(50f, 50f);

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 50f;
    [SerializeField] private float zoomSmoothness = 10f;
    [SerializeField] private float zoomHeightMultiplier = 0.5f; // How much height affects zoom speed

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float minRotation = 30f;  // Minimum angle from horizontal
    [SerializeField] private float maxRotation = 90f;  // Maximum angle (straight down)
    [SerializeField] private float rotationSmoothness = 10f;

    private Camera cam;
    private float targetZoom;
    private float targetRotation;
    private Vector3 lastMousePosition;
    private bool isRotating;
    private Vector3 targetPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        // Initialize target zoom based on camera's current position
        targetZoom = transform.position.y;
        targetPosition = transform.position;
        targetRotation = transform.eulerAngles.x;
    }

    private void Update()
    {
        if (!IsMouseInViewport()) return;

        HandleMovement();
        HandleZoom();
        HandleRotation();
        UpdatePosition();
    }

    private bool IsMouseInViewport()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.x <= Screen.width &&
               mousePos.y >= 0 && mousePos.y <= Screen.height;
    }

    private void HandleMovement()
    {
        if (isRotating) return;

        Vector3 pos = targetPosition;

        // Keyboard input
        if (Input.GetKey("w") || (IsMouseInViewport() && Input.mousePosition.y >= Screen.height - panBorderThickness))
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s") || (IsMouseInViewport() && Input.mousePosition.y <= panBorderThickness))
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("d") || (IsMouseInViewport() && Input.mousePosition.x >= Screen.width - panBorderThickness))
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("a") || (IsMouseInViewport() && Input.mousePosition.x <= panBorderThickness))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        // Clamp position within limits
        pos.x = Mathf.Clamp(pos.x, panLimitMin.x, panLimitMax.x);
        pos.z = Mathf.Clamp(pos.z, panLimitMin.y, panLimitMax.y);

        targetPosition = pos;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Adjust zoom speed based on current height for more natural feel
            float heightFactor = Mathf.Clamp01((targetZoom - minZoom) / (maxZoom - minZoom));
            float currentZoomSpeed = zoomSpeed * (1f + heightFactor * zoomHeightMultiplier);
            
            targetZoom -= scroll * currentZoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }

    private void HandleRotation()
    {
        // Start rotation on right mouse button down
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
        // End rotation on right mouse button up
        else if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        // Handle rotation while right mouse button is held
        if (isRotating)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            targetRotation += mouseDelta.y * rotationSpeed * Time.deltaTime;
            targetRotation = Mathf.Clamp(targetRotation, minRotation, maxRotation);
            lastMousePosition = Input.mousePosition;
        }

        // Smoothly rotate to target angle
        Vector3 currentRotation = transform.eulerAngles;
        float newRotation = Mathf.LerpAngle(currentRotation.x, targetRotation, Time.deltaTime * rotationSmoothness);
        transform.eulerAngles = new Vector3(newRotation, currentRotation.y, currentRotation.z);
    }

    private void UpdatePosition()
    {
        // Calculate the target position with the current zoom level
        Vector3 newPosition = targetPosition;
        newPosition.y = targetZoom;

        // Smoothly move to the target position
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * zoomSmoothness);
    }
} 