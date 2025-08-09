using UnityEngine;

public interface ISelectable
{
    /// <summary>
    /// Called when this object is selected by the player.
    /// </summary>
    bool OnSelected(in SelectionArgs args); // 'in' already passes SelectionArgs as readonly ref

    /// <summary>
    /// Called when this object is deselected (either another object is selected or selection is cleared).
    /// </summary>
    bool OnDeselected();

    /// <summary>
    /// Optional: Returns a display name or ID for debug/UI purposes.
    /// </summary>
    string GetSelectableName();
}
public readonly struct SelectionArgs
{
    public SelectionArgs(
        Vector3 worldPoint,
        Vector2 screenPoint,
        RaycastHit hit,
        bool isAdditive,
        bool isAlt,
        int mouseButton,
        Camera camera,
        Vector2Int? axial = null,
        Vector3? hexCenter = null,
        object source = null)
    {
        WorldPoint = worldPoint;
        ScreenPoint = screenPoint;
        Hit = hit;
        IsAdditive = isAdditive;
        IsAlt = isAlt;
        MouseButton = mouseButton;
        Camera = camera;
        Axial = axial;
        HexCenter = hexCenter;
        Source = source;
    }

    public Vector3 WorldPoint { get; }
    public Vector2 ScreenPoint { get; }
    public RaycastHit Hit { get; }
    public bool IsAdditive { get; }   // e.g., Shift
    public bool IsAlt { get; }        // e.g., Ctrl/Alt for special select
    public int MouseButton { get; }   // 0=LMB, 1=RMB, etc.
    public Camera Camera { get; }
    public Vector2Int? Axial { get; }
    public Vector3? HexCenter { get; } // Optional: center of hex if applicable, for visual feedback
    public object Source { get; }     // e.g., the InputRouter or player id
}