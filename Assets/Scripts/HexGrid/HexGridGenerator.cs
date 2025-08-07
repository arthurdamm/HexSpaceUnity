using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    public HexConfig config;
    public GameObject hexPrefab;
    // public int radius = 5; // Hex radius (in axial steps)
    // public float hexSize = 1f;

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("HexGridGenerator: no config");
        }
        for (int q = -config.gridRadius; q <= config.gridRadius; q++)
        {
            int r1 = Mathf.Max(-config.gridRadius, -q - config.gridRadius);
            int r2 = Mathf.Min(config.gridRadius, -q + config.gridRadius);
            for (int r = r1; r <= r2; r++)
            {
                // Vector3 pos = HexToWorld(q, r, config.hexSize);
                Vector3 pos = HexSpace.Utils.HexMath.AxialToWorld(q, r, config.hexSize);
                Instantiate(hexPrefab, pos, Quaternion.identity, transform);
            }
        }
    }

    // // Converts axial (q, r) to world position
    // Vector3 HexToWorld(int q, int r, float size)
    // {
    //     float x = size * 3f / 2f * q;
    //     float z = size * Mathf.Sqrt(3f) * (r + q / 2f);
    //     return new Vector3(x, 0, z);
    // }
}
