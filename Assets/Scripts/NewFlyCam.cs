using UnityEngine;
using UnityEngine.InputSystem;

public class NewFlyCam : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 100f;

    private Vector2 lookDelta;
    private Vector3 moveDir;

    void Update()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            lookDelta = Mouse.current.delta.ReadValue();
            transform.Rotate(Vector3.up, lookDelta.x * lookSpeed * Time.deltaTime, Space.World);
            transform.Rotate(Vector3.right, -lookDelta.y * lookSpeed * Time.deltaTime, Space.Self);
        }

        Vector3 move = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) move += transform.forward;
        if (Keyboard.current.sKey.isPressed) move -= transform.forward;
        if (Keyboard.current.aKey.isPressed) move -= transform.right;
        if (Keyboard.current.dKey.isPressed) move += transform.right;
        if (Keyboard.current.eKey.isPressed) move += transform.up;
        if (Keyboard.current.qKey.isPressed) move -= transform.up;

        transform.position += move.normalized * moveSpeed * Time.deltaTime;
    }
}