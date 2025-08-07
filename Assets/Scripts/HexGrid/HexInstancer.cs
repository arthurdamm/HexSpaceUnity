using UnityEngine;
using System.Collections.Generic;
using System;
// using System.Numerics;

public class HexInstancer : MonoBehaviour
{
    public HexConfig config;
    public Mesh hexMesh;
    public Material innerMaterial;
    public Material borderMaterial;
    private float hexWidth;
    public GameObject textLabelPrefab;
    private List<Matrix4x4> matrices = new List<Matrix4x4>();

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("HexGridGenerator: no config");
            return;
        }

        hexWidth = (float)Math.Sqrt(3) * config.hexSize;

        for (int q = -config.gridRadius; q <= config.gridRadius; q++)
        {
            int r1 = Mathf.Max(-config.gridRadius, -q - config.gridRadius);
            int r2 = Mathf.Min(config.gridRadius, -q + config.gridRadius);
            for (int r = r1; r <= r2; r++)
            {
                Vector3 pos = HexSpace.Utils.HexMath.AxialToWorld(q, r, config.hexSize); 
                Matrix4x4 mat = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
                matrices.Add(mat);
                CreateDebugLabel(pos, q, r);
            }
        }
        AddBoundingBoxCollider(this.gameObject);
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

    void AddBoundingBoxCollider(GameObject hexGridRoot)
    {
        BoxCollider collider = hexGridRoot.AddComponent<BoxCollider>();
        collider.size = new Vector3(2*gridHeight(), 0.001f, 2*gridWidth()); // Thin Y-axis so it doesn't interfere vertically
        collider.center = new Vector3(0f, 0f, 0f); // Centered at grid origin
        collider.isTrigger = false;
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

    public float gridHeight()
    {
        return (config.gridRadius + .5f) * hexWidth;
    }

    public float gridWidth()
    {
        return config.hexSize * (2 + 3 * config.gridRadius / 2);
    }

    private void CreateDebugLabel(Vector3 position, int q, int r)
    {
        if (textLabelPrefab == null) return;

        GameObject label = Instantiate(textLabelPrefab, position + Vector3.up * 0.05f, Quaternion.identity, transform);
        label.GetComponent<TextMesh>().text = $"({q},{r})";
        label.GetComponent<TextMesh>().fontSize = 20;
        label.GetComponent<TextMesh>().characterSize = 0.1f;
        label.GetComponent<TextMesh>().color = Color.white;

        // label.transform.LookAt(Camera.main.transform);
        label.transform.Rotate(90, 0, 0); // So text isn't backwards
    }

}