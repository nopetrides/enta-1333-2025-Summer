using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTS_CameraController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float scrollSpeed = 2000f;
    public Vector2 panLimit = new(50, 50);
    public float minY = 10f, maxY = 100f;
    public float rotateSpeed = 50f;

    void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
    }

    void HandleMovement()
    {
        Vector3 pos = transform.position;

        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down

        pos.x += h * moveSpeed * Time.deltaTime;
        pos.z += v * moveSpeed * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = transform.position;
        pos.y -= scroll * scrollSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            float rotY = -Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;

            transform.Rotate(new Vector3(rotX, rotY, 0), Space.Self);

            // Lock Z rotation to 0 to prevent camera flipping
            Vector3 angles = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(angles.x, angles.y, 0);
        }
    }
}
