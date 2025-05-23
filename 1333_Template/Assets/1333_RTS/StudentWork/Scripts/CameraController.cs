using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    // Movement speed in world units per second
    [Header("Movement Settings")]
    [SerializeField] private float panSpeed = 20f;
    // Thickness in pixels for screen edge panning
    [SerializeField] private float panBorderThickness = 10f;
    // Minimum X and Z limits for camera movement
    [SerializeField] private Vector2 panLimitMin = new Vector2(-50f, -50f);
    // Maximum X and Z limits for camera movement
    [SerializeField] private Vector2 panLimitMax = new Vector2(50f, 50f);

    // Zoom settings
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f; // Speed of zooming
    [SerializeField] private float minZoom = 10f;   // Minimum camera height
    [SerializeField] private float maxZoom = 50f;   // Maximum camera height
    [SerializeField] private float zoomSmoothness = 10f; // How smooth zooming is
    [SerializeField] private float zoomHeightMultiplier = 0.5f; // Height affects zoom speed

    // Rotation settings
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f; // Speed of rotation
    [SerializeField] private float minRotation = 30f;    // Minimum pitch angle
    [SerializeField] private float maxRotation = 90f;    // Maximum pitch angle
    [SerializeField] private float rotationSmoothness = 10f; // How smooth rotation is

    // Panning settings
    [Header("Panning Settings")]
    [SerializeField] private float panDragSpeed = 0.5f; // Speed of middle mouse panning
    [SerializeField] private float rotationSensitivity = 0.2f; // Sensitivity for right-click rotation

    // Private fields for camera state
    private Camera _cam;                // Reference to the Camera component
    private float _targetZoom;          // Target camera height
    private float _targetRotation;      // Target camera pitch
    private float _targetYaw;             // Target camera yaw (Y axis)
    private Vector3 _lastMousePosition; // Last mouse position for rotation
    private bool _isRotating;           // Is the camera currently rotating
    private Vector3 _targetPosition;    // Target camera position
    private Vector3 _lastPanMousePosition; // Last mouse position for middle mouse panning
    private bool _isPanning; // Is the camera currently panning

    private void Awake()
    {
        // Cache the Camera component (O(1) since RequireComponent ensures it exists)
        _cam = GetComponent<Camera>();
        // Set initial target zoom to current camera height
        _targetZoom = transform.position.y;
        // Set initial target position to current position
        _targetPosition = transform.position;
        // Set initial target rotation to current pitch (X axis)
        _targetRotation = transform.eulerAngles.x;
        // Set initial target yaw to current yaw (Y axis)
        _targetYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        // Only process input if mouse is inside the game window
        if (!IsMouseInViewport()) return;

        // Handle camera movement (WASD and screen edge)
        HandleMovement();
        // Handle camera zoom (mouse wheel)
        HandleZoom();
        // Handle camera rotation (right mouse button)
        HandleRotation();
        // Handle camera panning (middle mouse button)
        HandlePanning();
        // Smoothly update camera position and height
        UpdatePosition();
    }

    // Checks if the mouse is inside the game window
    private bool IsMouseInViewport()
    {
        Vector3 mousePos = Input.mousePosition;
        // Return true if mouse is within screen bounds
        return mousePos.x >= 0 && mousePos.x <= Screen.width &&
               mousePos.y >= 0 && mousePos.y <= Screen.height;
    }

    // Handles camera movement with WASD and screen edge
    private void HandleMovement()
    {
        // Start with current target position
        Vector3 pos = _targetPosition;

        // Move forward (W key or mouse at top edge)
        if (Input.GetKey("w") || (IsMouseInViewport() && Input.mousePosition.y >= Screen.height - panBorderThickness))
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        // Move backward (S key or mouse at bottom edge)
        if (Input.GetKey("s") || (IsMouseInViewport() && Input.mousePosition.y <= panBorderThickness))
        {
            pos.z -= panSpeed * Time.deltaTime;
        }
        // Move right (D key or mouse at right edge)
        if (Input.GetKey("d") || (IsMouseInViewport() && Input.mousePosition.x >= Screen.width - panBorderThickness))
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        // Move left (A key or mouse at left edge)
        if (Input.GetKey("a") || (IsMouseInViewport() && Input.mousePosition.x <= panBorderThickness))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        // Clamp X position within min and max limits
        pos.x = Mathf.Clamp(pos.x, panLimitMin.x, panLimitMax.x);
        // Clamp Z position within min and max limits
        pos.z = Mathf.Clamp(pos.z, panLimitMin.y, panLimitMax.y);

        // Set the new target position
        _targetPosition = pos;
    }

    // Handles camera zoom with mouse wheel
    private void HandleZoom()
    {
        // Get mouse scroll wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Calculate how much height affects zoom speed (for natural feel)
            float heightFactor = Mathf.Clamp01((_targetZoom - minZoom) / (maxZoom - minZoom));
            // Adjust zoom speed based on height
            float currentZoomSpeed = zoomSpeed * (1f + heightFactor * zoomHeightMultiplier);

            // Update target zoom based on scroll input
            _targetZoom -= scroll * currentZoomSpeed;
            // Clamp target zoom within min and max
            _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
        }
    }

    // Handles camera rotation with right mouse button (now both pitch and yaw)
    private void HandleRotation()
    {
        // Start rotating when right mouse button is pressed
        if (Input.GetMouseButtonDown(1))
        {
            _isRotating = true;
            // Store current mouse position
            _lastMousePosition = Input.mousePosition;
        }
        // Stop rotating when right mouse button is released
        else if (Input.GetMouseButtonUp(1))
        {
            _isRotating = false;
        }

        // If currently rotating
        if (_isRotating)
        {
            // Calculate mouse movement delta (raw, frame to frame)
            Vector3 mouseDelta = Input.mousePosition - _lastMousePosition;
            // Adjust target pitch (vertical, X axis) based on vertical mouse movement, scaled by sensitivity
            _targetRotation -= mouseDelta.y * rotationSensitivity;
            // Clamp target pitch within min and max
            _targetRotation = Mathf.Clamp(_targetRotation, minRotation, maxRotation);
            // Adjust target yaw (horizontal, Y axis) based on horizontal mouse movement, scaled by sensitivity
            _targetYaw += mouseDelta.x * rotationSensitivity;
            // Wrap yaw to 0-360
            if (_targetYaw < 0f) _targetYaw += 360f;
            if (_targetYaw >= 360f) _targetYaw -= 360f;
            // Update last mouse position
            _lastMousePosition = Input.mousePosition;
        }

        // Smoothly interpolate current rotation towards target pitch and yaw
        Vector3 currentRotation = transform.eulerAngles;
        float newPitch = Mathf.LerpAngle(currentRotation.x, _targetRotation, Time.deltaTime * rotationSmoothness);
        float newYaw = Mathf.LerpAngle(currentRotation.y, _targetYaw, Time.deltaTime * rotationSmoothness);
        // Apply new rotation (pitch and yaw, keep roll unchanged)
        transform.eulerAngles = new Vector3(newPitch, newYaw, currentRotation.z);
    }

    // Handles camera panning with middle mouse button
    private void HandlePanning()
    {
        // Start panning when middle mouse button is pressed
        if (Input.GetMouseButtonDown(2))
        {
            _isPanning = true;
            // Store current mouse position
            _lastPanMousePosition = Input.mousePosition;
        }
        // Stop panning when middle mouse button is released
        else if (Input.GetMouseButtonUp(2))
        {
            _isPanning = false;
        }

        // If currently panning
        if (_isPanning)
        {
            // Calculate mouse movement delta (raw, frame to frame)
            Vector3 mouseDelta = Input.mousePosition - _lastPanMousePosition;
            // Convert mouse delta to world movement (pan parallel to ground)
            Vector3 right = transform.right;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            // Move target position by mouse delta, scaled by panDragSpeed
            _targetPosition -= (right * mouseDelta.x + forward * mouseDelta.y) * panDragSpeed * Time.deltaTime;
            // Clamp X and Z position within min and max limits
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, panLimitMin.x, panLimitMax.x);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, panLimitMin.y, panLimitMax.y);
            // Update last pan mouse position
            _lastPanMousePosition = Input.mousePosition;
        }
    }

    // Smoothly updates camera position and height
    private void UpdatePosition()
    {
        // Set new position with current X/Z and target Y (zoom)
        Vector3 newPosition = _targetPosition;
        newPosition.y = _targetZoom;

        // Smoothly interpolate position towards target
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * zoomSmoothness);
    }
} 