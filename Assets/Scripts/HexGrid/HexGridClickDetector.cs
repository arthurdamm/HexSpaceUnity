using UnityEngine;

public class HexGridClickDetector : MonoBehaviour
{
    public Camera cam;  // Assign in inspector or via script
    public HexInstancer grid;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("DOWN");
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector2Int axial = HexSpace.Utils.HexMath.WorldToAxial(hit.point, 1f);
                Debug.Log($"Convert {hit.point} to Axial: {axial}");
                grid.ToggleHighlight(axial);
            }
        }
    }
}
