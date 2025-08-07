using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    public HexConfig config;
    public GameObject hexPrefab;

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
                Vector3 pos = HexSpace.Utils.HexMath.AxialToWorld(q, r, config.hexSize);
                Instantiate(hexPrefab, pos, Quaternion.identity, transform);
            }
        }
    }

}
