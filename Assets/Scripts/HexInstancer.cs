using UnityEngine;
using System.Collections.Generic;

public class HexInstancer : MonoBehaviour
{
    public Mesh hexMesh;
    public Material innerMaterial;
    public Material borderMaterial;
    public float hexSize = 1f;
    public int radius = 100;

    private List<Matrix4x4> matrices = new List<Matrix4x4>();

    void Start()
    {
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                Vector3 pos = AxialToWorld(q, r);
                Matrix4x4 mat = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * hexSize);
                matrices.Add(mat);
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < matrices.Count; i += 1023)
        {
            int count = Mathf.Min(1023, matrices.Count - i);
            Graphics.DrawMeshInstanced(hexMesh, 0, innerMaterial, matrices.GetRange(i, count));
            Graphics.DrawMeshInstanced(hexMesh, 1, borderMaterial, matrices.GetRange(i, count));
        }
    }

    Vector3 AxialToWorld(int q, int r)
    {
        float x = hexSize * 3f / 2f * q;
        float z = hexSize * Mathf.Sqrt(3f) * (r + q / 2f);
        return new Vector3(x, 0, z);
    }
}