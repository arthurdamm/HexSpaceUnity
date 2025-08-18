using UnityEngine;
using Unity.Cinemachine;

// Generated wrapper from your Input Actions asset
// Class name matches what you generated in step 2:
public class RtsCamera_WithInputActions : MonoBehaviour
{
    public Transform pivot;                // CamRigPivot
    public CinemachineCamera vcam;         // VCam_Gameplay

    [Header("Speeds")]
    public float panSpeed = 20f;           // units/sec
    public float yawPerPixel = 0.25f;      // deg per mouse pixel
    public float zoomSpeed = 18f;          // scalar

    [Header("Zoom Mode")]
    public bool zoomUsesDolly = false;     // true = PositionComposer.CameraDistance; false = FOV
    public float minFov = 28f, maxFov = 65f;
    public float minDist = 6f, maxDist = 80f;

    InputSystem_Actions inputs;            // <-- your generated class
    InputSystem_Actions.PlayerActions cam; // we'll use the Player map

    void Awake()
    {
        inputs = new InputSystem_Actions();
        cam = inputs.Player;
    }

    void OnEnable()  => cam.Enable();
    void OnDisable() => cam.Disable();

    void Update()
    {
        if (!pivot || !vcam) return;
        float dt = Time.deltaTime;

        // --- PAN (WASD / left stick)
        Vector2 move = cam.Move.ReadValue<Vector2>();
        if (move.sqrMagnitude > 0.0001f)
        {
            Vector3 fwd = Vector3.ProjectOnPlane(pivot.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(pivot.right, Vector3.up).normalized;
            pivot.position += (fwd * move.y + right * move.x) * panSpeed * dt;
        }

        // --- LOOK delta (mouse pixels or right stick). Gate with RMB for orbit.
        Vector2 look = cam.Look.ReadValue<Vector2>();
        bool orbit = cam.OrbitModifier.IsPressed();
        if (orbit && look.sqrMagnitude > 0.000001f)
        {
            float yaw = look.x * yawPerPixel; // horizontal orbit only
            pivot.Rotate(0f, yaw, 0f, Space.World);
        }

        // --- Middle-mouse DRAG PAN (gate + look delta)
        bool drag = cam.DragPanModifier.IsPressed();
        if (drag && look.sqrMagnitude > 0.000001f)
        {
            Vector2 px = look * 0.02f; // pixels -> world scale
            Vector3 fwd = Vector3.ProjectOnPlane(pivot.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(pivot.right, Vector3.up).normalized;
            pivot.position += (fwd * px.y + right * px.x) * panSpeed * dt;
        }

        // --- ZOOM (float axis by construction)
        // float z = cam.Zoom.ReadValue<float>();
        // if (Mathf.Abs(z) > 0.0001f)
        // {
        //     if (zoomUsesDolly && vcam.TryGetComponent<CinemachinePositionComposer>(out var pos))
        //     {
        //         float d = Mathf.Clamp(pos.CameraDistance - z * zoomSpeed * dt, minDist, maxDist);
        //         pos.CameraDistance = d;
        //     }
        //     else
        //     {
        //         var lens = vcam.Lens;
        //         lens.FieldOfView = Mathf.Clamp(lens.FieldOfView - z * (zoomSpeed * 0.8f) * dt, minFov, maxFov);
        //         vcam.Lens = lens;
        //     }
        // }
        // --- ZOOM (float axis by construction; no Time.deltaTime for wheel)
        float z = cam.Zoom.ReadValue<float>();
        if (Mathf.Abs(z) > 0.0001f)
        {
            if (zoomUsesDolly)
            {
                if (vcam.TryGetComponent<CinemachineThirdPersonFollow>(out var tpf))
                {
                    tpf.CameraDistance = Mathf.Clamp(
                        tpf.CameraDistance - z * zoomSpeed,
                        minDist, maxDist
                    );
                }
                else if (vcam.TryGetComponent<CinemachinePositionComposer>(out var pc))
                {
                    pc.CameraDistance = Mathf.Clamp(
                        pc.CameraDistance - z * zoomSpeed,
                        minDist, maxDist
                    );
                }
            }
            else
            {
                var lens = vcam.Lens;
                lens.FieldOfView = Mathf.Clamp(
                    lens.FieldOfView - z * (zoomSpeed * 0.5f),
                    minFov, maxFov
                );
                vcam.Lens = lens;
            }
        }

        // --- Optional center key
        if (cam.Center.WasPerformedThisFrame())
        {
            // call your selection to get world pos, then:
            // pivot.position = selectedWorldPos;
        }
    }

    // Public helpers if your selection system needs them:
    public void CenterOnPosition(Vector3 p) => pivot.position = p;
    public void CenterOnTransform(Transform t) { if (t) pivot.position = t.position; }
}