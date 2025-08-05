using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float movementSpeed = 10f;
    public float lookSpeed = 2f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Debug.Log("Start ran");
    }

    void Update()
    {
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) direction += transform.forward;
        if (Input.GetKey(KeyCode.S)) direction -= transform.forward;
        if (Input.GetKey(KeyCode.A)) direction -= transform.right;
        if (Input.GetKey(KeyCode.D)) direction += transform.right;
        if (Input.GetKey(KeyCode.E)) direction += transform.up;
        if (Input.GetKey(KeyCode.Q)) direction -= transform.up;

        transform.position += direction.normalized * movementSpeed * Time.deltaTime;

        Debug.Log("Flying"); // Add this line to check if Update() is firing
    }
}