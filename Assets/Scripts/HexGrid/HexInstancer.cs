using UnityEngine;
using System.Collections.Generic;
using System;

public class HexInstancer : MonoBehaviour
{
    public HexConfig config;
    public Mesh hexMesh;
    public Material innerMaterial;
    public Material borderMaterial;
    public Material borderHighlightMaterial;

    public GameObject textLabelPrefab;
    private float hexWidth;
    private List<Matrix4x4> defaultMatrices = new List<Matrix4x4>();
    private List<Matrix4x4> highlightMatrices = new();
    private List<Matrix4x4> testMatrices = new();

    // Mapping axial coords to their world position and highlight state
    private Dictionary<Vector2Int, Vector3> axialToWorld = new();
    private HashSet<Vector2Int> highlighted = new();
    private List<GameObject> textLabels = new();

    void OnEnable()
    {
        RebuildGrid();
        AddBoundingBoxCollider(this.gameObject);
    }

    void Start()
    {

    }

    public void RebuildGrid(bool showLabel = true)
    {
        // Clean up existing labels
        if (showLabel)
        {
            foreach (var label in textLabels)
            {
                if (label != null)
                {
                    Destroy(label); // Immediate because we're in edit mode
                }
            }
            textLabels.Clear();
        }
        

        if (config == null)
        {
            Debug.LogError("HexGridGenerator: no config");
            return;
        }

        defaultMatrices.Clear();
        highlightMatrices.Clear();
        axialToWorld.Clear();

        float hexSize = config.hexSize;
        int gridRadius = config.gridRadius;
        hexWidth = hexSize * (float)Math.Sqrt(3);

        for (int q = -gridRadius; q <= gridRadius; q++)
        {
            int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
            int r2 = Mathf.Min(gridRadius, -q + gridRadius);
            for (int r = r1; r <= r2; r++)
            {
                Vector2Int axial = new(q, r);
                Vector3 pos = HexSpace.Utils.HexMath.AxialToWorld(q, r, hexSize);
                axialToWorld[axial] = pos;
                Matrix4x4 mat = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);

                if (highlighted.Contains(axial))
                {
                    highlightMatrices.Add(mat);
                }
                else
                {
                    defaultMatrices.Add(mat);
                }

                if (showLabel)
                {
                    CreateDebugLabel(pos, q, r);
                }
            }
        }

    }

    void AddBoundingBoxCollider(GameObject hexGridRoot)
    {
        Debug.Log($"h:{gridHeight()} w:{gridWidth()}");
        BoxCollider collider = hexGridRoot.AddComponent<BoxCollider>();
        collider.size = new Vector3(2 * gridHeight(), 0.001f, 2 * gridWidth()); // Thin Y-axis so it doesn't interfere vertically
        collider.center = new Vector3(0f, 0f, 0f); // Centered at grid origin
        collider.isTrigger = false;
    }

    void Update()
    {
        for (int i = 0, j = 0; i < defaultMatrices.Count; i += 1023, j++)
        {
            int count = Mathf.Min(1023, defaultMatrices.Count - i);
            Graphics.DrawMeshInstanced(hexMesh, 0, innerMaterial, defaultMatrices.GetRange(i, count));
            Graphics.DrawMeshInstanced(hexMesh, 1, borderMaterial, defaultMatrices.GetRange(i, count));
        }

        for (int i = 0; i < highlightMatrices.Count; i += 1023)
        {
            int count = Mathf.Min(1023, highlightMatrices.Count - i);
            Graphics.DrawMeshInstanced(hexMesh, 0, innerMaterial, highlightMatrices.GetRange(i, count));
            Graphics.DrawMeshInstanced(hexMesh, 1, borderHighlightMaterial, highlightMatrices.GetRange(i, count));
        }
        
        for (int i = 0; i < testMatrices.Count; i += 1023)
        {
            int count = Mathf.Min(1023, testMatrices.Count - i);
            Graphics.DrawMeshInstanced(hexMesh, 0, innerMaterial, testMatrices.GetRange(i, count));
            Graphics.DrawMeshInstanced(hexMesh, 1, borderHighlightMaterial, testMatrices.GetRange(i, count));
        }
    }

    public void ToggleHighlight(Vector2Int axial)
    {
        if (highlighted.Contains(axial))
        {
            highlighted.Remove(axial);
        }
        else
        {
            highlighted.Add(axial);
        }

        RebuildGrid(false); // re-bake the matrices
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
        
        textLabels.Add(label);

    }

}