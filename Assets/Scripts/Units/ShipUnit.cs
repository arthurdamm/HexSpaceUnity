using UnityEngine;

public class ShipUnit : MonoBehaviour, ISelectable
{
    public string ShipName = "ShipUnit";
    public int OwnerID;
    public Vector2Int Axial; // Assuming hex grid uses axial coords
    public int MovementPoints = 3;


    public void MoveTo(Vector2Int newCoord)
    {
        // You’ll connect this to a GridManager later
        Debug.Log($"{ShipName} moving from {Axial} to {newCoord}");
        Axial = newCoord;
        // Update visual position here too
    }

    public string GetSelectableName()
    {
        return ShipName;
    }

    public bool OnDeselected()
    {
        return false;
    }

    public bool OnSelected(in SelectionArgs args)
    {
        Debug.Log($"Ship {ShipName} selected!");
        return true;
    }
}