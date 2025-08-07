using UnityEngine;

public class HexGridClickDetector : MonoBehaviour
{
    public Camera cam;  // Assign in inspector or via script
    public HexInstancer grid;
    public HexCameraFollower cameraFollower;


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector2Int axial = HexSpace.Utils.HexMath.WorldToAxial(hit.point, 1f);
                Debug.Log($"Hit Point convert {hit.point} to Axial: {axial}");
                grid.ToggleHighlight(axial);

                if (grid.TryGetWorldPosition(axial, out Vector3 hexCenter))
                {
                    cameraFollower.CenterOnHex(hexCenter);
                }
            }
        }
    }
}
