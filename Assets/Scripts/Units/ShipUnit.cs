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

    [Header("Movement Tuning")]
    [Tooltip("World units per second along the path")]
    public float MoveSpeed = 6f;
    [Tooltip("Extra easing at the ends (0 = linear, 1 = strong ease)")]
    [Range(0f, 1f)] public float Ease = 0.25f;

    // Minimal global selection for the demo loop
    public static ShipUnit Selected { get; private set; }

    private bool _isMoving;
    private Coroutine _moveCo;

    public string GetSelectableName() => ShipName;

    public bool OnSelected(in SelectionArgs args)
    {
        Debug.Log($"Ship {ShipName} selected!");
        Selected = this;
        return true;
    }

    public bool OnDeselected()
    {
        if (Selected == this) Selected = null;
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

        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, worldTarget);
        // Derive duration from distance and speed; clamp to avoid zero-duration
        float duration = Mathf.Max(0.0001f, distance / Mathf.Max(0.001f, MoveSpeed));

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = ApplyEase(Mathf.Clamp01(t), Ease);
            transform.position = Vector3.LerpUnclamped(start, worldTarget, eased);
            yield return null;
        }

        Axial = targetAxial; // commit logical position when we arrive
        transform.position = worldTarget; // snap to center
        _isMoving = false;
        _moveCo = null;
    }

    // Smoothstep-like adjustable ease-in/out
    private static float ApplyEase(float x, float ease)
    {
        // base smoothstep
        float s = x * x * (3f - 2f * x);
        return Mathf.Lerp(x, s, ease);
    }
}