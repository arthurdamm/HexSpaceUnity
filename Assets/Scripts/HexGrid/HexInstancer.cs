using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
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

    // void AddBoundingBoxCollider(GameObject hexGridRoot)
    // {
    //     Debug.Log($"h:{gridHeight()} w:{gridWidth()}");
    //     BoxCollider collider = hexGridRoot.AddComponent<BoxCollider>();
    //     collider.size = new Vector3(2 * gridHeight(), 0.001f, 2 * gridWidth()); // Thin Y-axis so it doesn't interfere vertically
    //     collider.center = Vector3.zero; // Centered at grid origin
    //     collider.isTrigger = false;
    // }

    void AddBoundingBoxCollider(GameObject hexGridRoot)
    {
        if (hexGridRoot.TryGetComponent<BoxCollider>(out var existingCollider))
        {
    #if UNITY_EDITOR
            DestroyImmediate(existingCollider);
    #else
            Destroy(existingCollider);
    #endif
        }

        Debug.Log($"Adding BoxCollider → h:{gridHeight()} w:{gridWidth()}");

        BoxCollider collider = hexGridRoot.AddComponent<BoxCollider>();
        collider.size = new Vector3(2 * gridHeight(), 0.001f, 2 * gridWidth());
        collider.center = Vector3.zero;
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

    public bool TryGetWorldPosition(Vector2Int axial, out Vector3 pos)
    {
        return axialToWorld.TryGetValue(axial, out pos);
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

    // private void CreateDebugLabel(Vector3 position, int q, int r)
    // {
    //     if (textLabelPrefab == null) return;

    //     GameObject label = Instantiate(textLabelPrefab, position + Vector3.up * 0.05f, Quaternion.identity, transform);
    //     label.GetComponent<TextMesh>().text = $"({q},{r})";
    //     label.GetComponent<TextMesh>().fontSize = 20;
    //     label.GetComponent<TextMesh>().characterSize = 0.1f;
    //     label.GetComponent<TextMesh>().color = Color.white;

        
    //     label.transform.Rotate(90, 0, 0); // So text isn't backwards

    //     textLabels.Add(label);

    // }

    private void CreateDebugLabel(Vector3 position, int q, int r)
    {
        if (textLabelPrefab == null) return;

        string labelName = $"DebugLabel_{q}_{r}";

        // Check if label already exists
        Transform existing = transform.Find(labelName);
        if (existing != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(existing.gameObject);
#else
        Destroy(existing.gameObject);
#endif
        }

        GameObject label = Instantiate(textLabelPrefab, position + Vector3.up * 0.05f, Quaternion.identity, transform);
        label.name = labelName;

        var text = label.GetComponent<TextMesh>();
        text.text = $"({q},{r})";
        text.fontSize = 20;
        text.characterSize = 0.1f;
        text.color = Color.white;

        // label.transform.LookAt(Camera.main.transform);
        label.transform.Rotate(90, 0, 0);

        textLabels.Add(label);
    }


    [ContextMenu("Clean Up Editor Junk")]
    public void CleanUpEditorJunk()
    {
#if UNITY_EDITOR
        Debug.Log("🧹 Cleaning up old BoxColliders and TextMesh labels...");

        // 1. Remove all BoxColliders attached to this GameObject
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        foreach (var col in colliders)
        {
            DestroyImmediate(col);
        }

        // 2. Find all child objects with a TextMesh component and delete their GameObjects
        TextMesh[] textMeshes = GetComponentsInChildren<TextMesh>(true);
        foreach (var tm in textMeshes)
        {
            DestroyImmediate(tm.gameObject);
        }

        // 3. Optional: clear lingering label refs
        textLabels.Clear();

        // 4. Mark scene as dirty so Unity knows to save changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);

        Debug.Log("✅ Editor junk cleared.");
#endif
    }

}