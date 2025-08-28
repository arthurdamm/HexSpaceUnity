using UnityEngine;
using Unity.Cinemachine;

public class RtsCamera_WithInputActions : MonoBehaviour
{
    // --- Rig references
    public Transform movePivot;                 // pans (root)
    public Transform yawPivot;                  // rotates Y (child of movePivot)
    public Transform pitchPivot;                // rotates X (child of yawPivot)  <-- VCam follows/looks at THIS
    public CinemachineCamera vcam;              // VCam_Gameplay

    // --- Speeds
    public float panSpeed = 24f;
    public float yawPerPixel = 0.30f;           // RMB orbit
    public float dragScale = 0.03f;             // MMB drag => world units per pixel

    // --- Zoom state in [0..1] driving distance/arm/pitch via curves
    [Range(0f,1f)] public float zoomT = 0.5f;   // 0=far top-down, 1=close shallow angle
    public float zoomStepPerNotch = 0.07f;      // how fast scroll changes zoomT

    // Distance (ThirdPersonFollow)
    public AnimationCurve distanceByT = AnimationCurve.EaseInOut(0, 36f, 1, 14f);
    // Vertical arm length (higher arm = higher vantage)
    public AnimationCurve armByT      = AnimationCurve.EaseInOut(0, 12f, 1, 4f);
    // Pitch (deg). Bigger X = look more downward.
    public AnimationCurve pitchByT    = AnimationCurve.EaseInOut(0, 55f, 1, 22f);

    // Fallback limits if you ever switch to PositionComposer
    public float minFov = 28f, maxFov = 65f;

    // Your generated input wrapper
    InputSystem_Actions inputs;
    InputSystem_Actions.PlayerActions cam;

    void Awake()
    {
        inputs = new InputSystem_Actions();
        cam = inputs.Player;
    }
    void OnEnable()  => cam.Enable();
    void OnDisable() => cam.Disable();

    void Start() { ApplyZoomRig(); }            // initialize TPF with curves

    void Update()
    {
        if (!movePivot || !yawPivot || !pitchPivot || !vcam) return;

        float dt = Time.deltaTime;

        // --- PAN (WASD)
        Vector2 move = cam.Move.ReadValue<Vector2>();
        if (move.sqrMagnitude > 0.0001f)
        {
            // pan in camera-facing plane using yawPivot forward/right (ignore pitch)
            Vector3 fwd   = Vector3.ProjectOnPlane(yawPivot.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(yawPivot.right,   Vector3.up).normalized;
            movePivot.position += (fwd * move.y + right * move.x) * panSpeed * dt;
        }

        // --- ORBIT (RMB + Look.x)
        Vector2 look = cam.Look.ReadValue<Vector2>();     // pixels for mouse
        if (cam.OrbitModifier.IsPressed() && Mathf.Abs(look.x) > 0.001f)
        {
            float yaw = look.x * yawPerPixel;
            yawPivot.Rotate(0f, yaw, 0f, Space.World);
        }

        // --- DRAG PAN (MMB + Look)
        if (cam.DragPanModifier.IsPressed() && look.sqrMagnitude > 0.000001f)
        {
            Vector2 px = look * dragScale;
            Vector3 fwd   = Vector3.ProjectOnPlane(yawPivot.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(yawPivot.right,   Vector3.up).normalized;
            movePivot.position += (fwd * px.y + right * px.x) * panSpeed * dt;
        }

        // --- ZOOM (scroll/triggers) -> adjust zoomT, then apply curves
        float z = cam.Zoom.ReadValue<float>();
        if (Mathf.Abs(z) > 0.0001f)
        {
            // IMPORTANT: do not multiply scroll by dt
            zoomT = Mathf.Clamp01(zoomT + z * zoomStepPerNotch);
            ApplyZoomRig();
        }

        // (Optional) quick center key
        if (cam.Center.WasPerformedThisFrame())
        {
            // e.g., movePivot.position = selectedWorldPos;
        }
    }

    void ApplyZoomRig()
    {
        // Drive TPF distance + arm length and the pitch pivot angle
        if (vcam.TryGetComponent<CinemachineThirdPersonFollow>(out var tpf))
        {
            tpf.CameraDistance    = distanceByT.Evaluate(zoomT);
            tpf.VerticalArmLength = armByT.Evaluate(zoomT);
        }
        else if (vcam.TryGetComponent<CinemachinePositionComposer>(out var pc))
        {
            // fallback if you ever swap components
            var lens = vcam.Lens;
            lens.FieldOfView = Mathf.Lerp(maxFov, minFov, zoomT);
            vcam.Lens = lens;
            pc.CameraDistance = distanceByT.Evaluate(zoomT);
        }

        // Pitch about local X (positive down). Clamp to a friendly range.
        float pitch = pitchByT.Evaluate(zoomT);
        var e = pitchPivot.localEulerAngles;
        e.x = pitch;
        pitchPivot.localEulerAngles = e;
    }

    // Public helpers for selection system
    public void CenterOnPosition(Vector3 p) => movePivot.position = p;
    public void CenterOnTransform(Transform t) { if (t) movePivot.position = t.position; }
}