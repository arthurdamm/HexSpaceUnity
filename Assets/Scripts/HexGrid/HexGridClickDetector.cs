using UnityEngine;

public class HexGridClickDetector : MonoBehaviour, ISelectable
{
    public Camera cam;  // Assign in inspector or via script
    public HexInstancer grid;
    public HexCameraFollower cameraFollower;

    public string GetSelectableName()
    {
        return "HexGridClickDetector";
    }

    public bool OnDeselected()
    {
        return true;
    }

    public bool OnSelected(in SelectionArgs args)
    {
        if (args.Axial == null || args.HexCenter == null)
        {
            return false;
        }
        grid.ToggleHighlight((Vector2Int)args.Axial);
        cameraFollower.CenterOnHex((Vector3)args.HexCenter);
        return true;
    }

}
