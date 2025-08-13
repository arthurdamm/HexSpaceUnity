using System.Collections;
using UnityEngine;

public class ShipUnit : MonoBehaviour, ISelectable
{
    [Header("Identity")]
    public string ShipName = "ShipUnit";
    public int OwnerID;

    [Header("Grid State")]
    public Vector2Int Axial; // current axial hex
    public int MovementPoints = 3;
    public HexDirection Facing = HexDirection.East; // current facing direction

    [Header("Movement Tuning")]
    [Tooltip("World units per second along the path")]
    public float MoveSpeed = 20f;
    [Tooltip("Extra easing at the ends (0 = linear, 1 = strong ease)")]
    [Range(0f, 1f)] public float Ease = 0.25f;
    [Tooltip("How fast the ship rotates (degrees per second)")]
    public float RotationSpeed = 180f;

    // Event triggered when the ship arrives at its destination
    public event System.Action<ShipUnit> Arrived;

    private bool _isMoving;
    private Coroutine _moveCo;

    void Start()
    {
        // Initialize rotation to match facing direction
        // transform.rotation = Facing.GetRotation();
    }

    public string GetSelectableName() => ShipName;

    public bool OnSelected(in SelectionArgs args)
    {
        Debug.Log($"Ship {ShipName} selected!");
        return true;
    }

    public bool OnDeselected()
    {
        return true;
    }

    /// <summary>
    /// Command the ship to move to a target hex (single step for now).
    /// </summary>
    public void CommandMove(Vector2Int targetAxial, Vector3 worldTarget)
    {
        if (_isMoving)
            return;

        if (_moveCo != null)
            StopCoroutine(_moveCo);

        _moveCo = StartCoroutine(MoveToCo(targetAxial, worldTarget));
    }

    private IEnumerator MoveToCo(Vector2Int targetAxial, Vector3 worldTarget)
    {
        _isMoving = true;

        // Calculate the direction we're moving
        HexDirection moveDirection = HexDirectionExtensions.GetDirectionTo(Axial, targetAxial);
        
        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = moveDirection.GetRotation();

        float distance = Vector3.Distance(startPos, worldTarget);
        float moveDuration = Mathf.Max(0.0001f, distance / Mathf.Max(0.001f, MoveSpeed));
        
        // Calculate rotation duration based on angle difference
        float angleDifference = Quaternion.Angle(startRotation, targetRotation);
        float rotationDuration = angleDifference / RotationSpeed;
        
        // Use the longer of the two durations to ensure both complete
        float totalDuration = Mathf.Max(moveDuration, rotationDuration);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / totalDuration;
            float eased = ApplyEase(Mathf.Clamp01(t), Ease);
            
            // Smoothly interpolate position
            transform.position = Vector3.LerpUnclamped(startPos, worldTarget, eased);
            
            // Smoothly interpolate rotation
            transform.rotation = Quaternion.LerpUnclamped(startRotation, targetRotation, eased);
            
            yield return null;
        }

        // Commit final state
        Axial = targetAxial;
        Facing = moveDirection;
        transform.position = worldTarget;
        transform.rotation = targetRotation;
        
        _isMoving = false;
        _moveCo = null;

        Arrived?.Invoke(this);
    }

    // Smoothstep-like adjustable ease-in/out
    private static float ApplyEase(float x, float ease)
    {
        // base smoothstep
        float s = x * x * (3f - 2f * x);
        return Mathf.Lerp(x, s, ease);
    }

    /// <summary>
    /// Immediately face a specific direction (useful for setup or instant turns)
    /// </summary>
    public void SetFacing(HexDirection direction)
    {
        if (!_isMoving)
        {
            Facing = direction;
            transform.rotation = direction.GetRotation();
        }
    }

    /// <summary>
    /// Get the hex coordinate in the direction the ship is currently facing
    /// </summary>
    public Vector2Int GetHexInFront()
    {
        return Axial + Facing.GetAxialOffset();
    }
}