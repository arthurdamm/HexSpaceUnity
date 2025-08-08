using UnityEngine;

public class HexCameraFollower : MonoBehaviour
{
    public float followSpeed = 5f;
    public float snapDistance = 0.01f;
    public float height = 10f;
    public float pitchAngle = 60f; // Tilt downward
    public float backDistance = 3f;

    private Vector3 targetPosition;
    private bool shouldFollow = false;

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (shouldFollow)
        {
            Vector3 current = transform.position;
            Vector3 next = Vector3.Lerp(current, targetPosition, Time.deltaTime * followSpeed);

            if ((next - targetPosition).sqrMagnitude < snapDistance * snapDistance)
            {
                next = targetPosition;
                shouldFollow = false;
            }

            transform.position = next;
        }
    }

    public void CenterOnHex(Vector3 hexCenter)
    {
        // Position camera above the hex and apply a pitch rotation
        targetPosition = hexCenter + Vector3.up * height + Vector3.back * backDistance;

        // Immediately rotate camera to look down at hex
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);

        shouldFollow = true;
    }
}
