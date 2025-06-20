using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_RTS : MonoBehaviour
{

    public float moveSpeed = 20f;       // Speed of camera movement
    public float rotateSpeed = 100f;    // Speed of camera rotation
    public float zoomSpeed = 5f;        // Speed of zooming in and out
    public float minZoom = 5f;          // Minimum zoom limit
    public float maxZoom = 80;        // Maximum zoom limit
    public float verticalSpeed = 5f;    // Speed of camera vertical movement

    private Camera cam;

    void Start()
    {
        // Cache reference to the main camera
        cam = Camera.main;
    }

    void Update()
    {
        // Store current position
        Vector3 pos = transform.position;

        // Forward movement (W key)
        if (Input.GetKey("w"))
        {
            pos.z += moveSpeed * Time.deltaTime;
        }
        transform.position = pos;

        // Backward movement (S key)
        if (Input.GetKey("s"))
        {
            pos.z -= moveSpeed * Time.deltaTime;
        }
        transform.position = pos;

        // Right movement (D key)
        if (Input.GetKey("d"))
        {
            pos.x += moveSpeed * Time.deltaTime;
        }
        transform.position = pos;

        // Left movement (A key)
        if (Input.GetKey("a"))
        {
            pos.x -= moveSpeed * Time.deltaTime;
        }
        transform.position = pos;

        // Move the camera downward (Left Control key)
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime, Space.World);
        }

        // Move the camera upward (Space key)
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * verticalSpeed * Time.deltaTime, Space.World);
        }

        // Zoom in/out with mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * zoomSpeed, minZoom, maxZoom);

        // Set zoom to max (comma key)
        if (Input.GetKey(","))
        {
            cam.fieldOfView = 80f;
        }

        // Set zoom to min (period key)
        if (Input.GetKey("."))
        {
            cam.fieldOfView = 5f;
        }
    }
}

