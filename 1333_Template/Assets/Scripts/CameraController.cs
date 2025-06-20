
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;



    void Update()
    { 

        Vector3 pos = transform.position;

        if (Input.GetKey(KeyCode.W)|| Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos.z += panSpeed * Time.deltaTime;

        }
        

        if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= panBorderThickness)
        {
            pos.z -= panSpeed * Time.deltaTime;

        }
        

        if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
            pos.x += panSpeed * Time.deltaTime;

        }
        

        if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= panBorderThickness)
        {
            pos.x -= panSpeed * Time.deltaTime;

        }

        transform.position = pos;
    }

    // Update is called once per frame

}
