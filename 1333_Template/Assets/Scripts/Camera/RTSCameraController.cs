using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class RTSCameraController : MonoBehaviour
{
    [Header("Panning")]
    public bool useKeyboardPan = true;
    public bool useMouseDragPan = true;
    public float panSpeed = 20f;
    public float dragSpeed = 0.5f;

    [Header("Zooming")]
    public float scrollZoomSpeed = 20f;
    public float verticalZoomSpeed = 20f;
    public float minHeight = 10f;
    public float maxHeight = 80f;

    [Header("Rotating")]
    public bool useKeyboardRotate = true;
    public float initialFocusDistance = 20f;
    public float rotateSpeed = 50f;

    // Imaginary pivot position for camera pointing
    private Vector3 pivot;

    void Start()
    {
        pivot = transform.position + transform.forward * initialFocusDistance;
    }

    void Update()
    {
        // Camera-relative axes
        Vector3 right = transform.right; right.y = 0; right.Normalize();
        Vector3 forward = transform.forward; forward.y = 0; forward.Normalize();

        // Track old pos so we can keep pivot synced
        Vector3 oldPos = transform.position;
        Vector3 pos = oldPos;

        // Keyboard pan (now camera-relative)
        if (useKeyboardPan)
        {
            Vector2 input = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1;

            if (input.sqrMagnitude > 0.01f)
            {
                Vector3 dir = (right * input.x + forward * input.y).normalized;
                pos += dir * panSpeed * Time.deltaTime;
            }
        }

        // Mouse-drag pan: only when holding Spacebar AND right mouse button
        if (useMouseDragPan
            && Mouse.current.middleButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            Vector3 drag = (right * -delta.x + forward * -delta.y) * dragSpeed * Time.deltaTime;
            pos += drag;
        }

        // Scroll zoom
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
            pos += transform.forward * scroll * scrollZoomSpeed * Time.deltaTime;

        // Vertical zoom
        if (Keyboard.current.fKey.isPressed) pos.y -= verticalZoomSpeed * Time.deltaTime;
        if (Keyboard.current.rKey.isPressed) pos.y += verticalZoomSpeed * Time.deltaTime;

        // Clamp height & apply
        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
        transform.position = pos;

        // Sync pivot
        pivot += (pos - oldPos);

        // Rotate
        if (useKeyboardRotate)
        {
            if (Keyboard.current.qKey.isPressed)
                transform.RotateAround(pivot, Vector3.up, rotateSpeed * Time.deltaTime);
            if (Keyboard.current.eKey.isPressed)
                transform.RotateAround(pivot, Vector3.up, -rotateSpeed * Time.deltaTime);
        }

        // Always look at the floating pivot
        transform.LookAt(pivot, Vector3.up);
    }
}
