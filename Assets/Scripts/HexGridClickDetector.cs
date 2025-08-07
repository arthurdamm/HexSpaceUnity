using UnityEngine;

public class HexGridClickDetector : MonoBehaviour
{
    public Camera cam;  // Assign in inspector or via script

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector2Int axial = HexMath.PixelToPointyHex(hit.point, 1f);
                // Debug.Log($"Axial is : {axial}");
                Debug.Log($"Convert {hit.point} to Axial: {axial}");
                // TODO: Convert hit.point to axial coords
            }
        }
    }
}

public static class HexMath
{
    public static Vector2Int AxialRoundBranchless(float x, float y)
    {
        float xgrid = Mathf.Round(x);
        float ygrid = Mathf.Round(y);

        x -= xgrid;
        y -= ygrid;
        float dx = Mathf.Round(x + 0.5f * y) * ((y * y) <= (x * x) ? 1 : 0);
        float dy = Mathf.Round(y + 0.5f * x) * ((x * x) <= (y * y) ? 1 : 0);
        xgrid += dx;
        ygrid += dy;

        int finalX = Mathf.RoundToInt(xgrid);
        int finalY = Mathf.RoundToInt(ygrid);
        Debug.Log($"Round ({xgrid}, {ygrid}) to ({finalX},{finalY})");
        return new Vector2Int(finalX, finalY);
    }

    public static Vector2Int PixelToPointyHex(Vector3 worldPos, float size)
    {
        Debug.Log("CALLING FUNC");
        float q = (Mathf.Sqrt(3f) / 3f * worldPos.x - 1f / 3f * worldPos.z) / size;
        float r = (2f / 3f * worldPos.z) / size;

        return AxialRoundBranchless(q, r);
    }
}