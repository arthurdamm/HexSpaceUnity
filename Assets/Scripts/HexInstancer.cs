using UnityEngine;
using System.Collections.Generic;
using System;

public class HexInstancer : MonoBehaviour
{
    public Mesh hexMesh;
    public Material innerMaterial;
    public Material borderMaterial;
    public float hexSize = 1f;
    private float hexWidth;
    public int gridRadius = 3;

    private List<Matrix4x4> matrices = new List<Matrix4x4>();

    void Start()
    {
        hexWidth = (float)Math.Sqrt(3) * hexSize;

        for (int q = -gridRadius; q <= gridRadius; q++)
        {
            int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
            int r2 = Mathf.Min(gridRadius, -q + gridRadius);
            for (int r = r1; r <= r2; r++)
            {
                Vector3 pos = AxialToWorld(q, r);
                Matrix4x4 mat = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * hexSize);
                matrices.Add(mat);
            }
        }
        AddBoundingBoxCollider(this.gameObject, hexSize, gridRadius);
    }

    public float gridHeight()
    {
        return hexSize * (1 + 3 * gridRadius / 2);
    }

    public float gridWidth()
    {
        return (gridRadius + .5f) * hexWidth;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(0f, 1f, 0f), new Vector3(0f, 1f, 1f));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(gridHeight(), .1f, -gridWidth()), new Vector3(gridHeight(), .1f, gridWidth()));
        Gizmos.DrawLine(new Vector3(-gridHeight(), .1f, -gridWidth()), new Vector3(-gridHeight(), .1f, gridWidth()));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(gridHeight(), .1f, gridWidth()), new Vector3(-gridHeight(), .1f, gridWidth()));
        Gizmos.DrawLine(new Vector3(gridHeight(), .1f, -gridWidth()), new Vector3(-gridHeight(), .1f, -gridWidth()));

        Gizmos.color = Color.black;
        Gizmos.DrawLine(new Vector3(0f, .1f, 0f), new Vector3(gridHeight(), .1f, 0f));

        Gizmos.color = Color.black;
        Gizmos.DrawLine(new Vector3(0f, .1f, 0f), new Vector3(0f, .1f, gridWidth()));
        
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

    void AddBoundingBoxCollider(GameObject hexGridRoot, float hexSize, int radius)
    {
        float width = hexSize * 3f * radius;
        float height = hexSize * Mathf.Sqrt(3f) * (2f * radius + 1f);

        BoxCollider collider = hexGridRoot.AddComponent<BoxCollider>();
        collider.size = new Vector3(2*gridHeight(), 0.1f, 2*gridWidth()); // Thin Y-axis so it doesn't interfere vertically
        collider.center = new Vector3(0f, 0f, 0f); // Centered at grid origin
        Debug.Log("AddBoundingBoxCollider");
    }
}