using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings for Movement")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Settings for Zoom")]
    [SerializeField] private float scrollSpeed = 10f;
    [SerializeField] private float minY = 10f;
    [SerializeField] private float maxY = 80f;

    private Vector3 dragOrigin;
    private bool isRotating = false;

    void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector3 pos = transform.position;           // wasds movement


        if (Input.GetKey("w")) pos += transform.forward * panSpeed * Time.deltaTime;
        if (Input.GetKey("s")) pos -= transform.forward * panSpeed * Time.deltaTime;
        if (Input.GetKey("d")) pos += transform.right * panSpeed * Time.deltaTime;
        if (Input.GetKey("a")) pos -= transform.right * panSpeed * Time.deltaTime;

        transform.position = pos;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = transform.position;

        pos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(2)) // scroll wheel held
        {
            dragOrigin = Input.mousePosition;
            isRotating = true;
        }
        if (Input.GetMouseButtonUp(2)) 
        {
            isRotating = false;
        }

        if (isRotating)
        {
            Vector3 delta = Input.mousePosition - dragOrigin;
            dragOrigin = Input.mousePosition;

            float rotationY = delta.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, rotationY, 0f, Space.World);
        }
    }
}
