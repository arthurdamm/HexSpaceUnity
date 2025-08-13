// Assets/Editor/BakeColliderChildren.cs
using UnityEditor;
using UnityEngine;

public static class BakeColliderChildren
{
    [MenuItem("Tools/Colliders/Bake Children Transforms (Preserve World)")]
    static void Bake()
    {
        var root = Selection.activeTransform;
        if (!root) { Debug.LogWarning("Select the 'Colliders' parent object."); return; }

        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Bake collider children");

        var parent = root.parent;
        int n = root.childCount;
        var kids = new Transform[n];
        for (int i = 0; i < n; i++) kids[i] = root.GetChild(i);

        // Reparent children to the original parent, preserving world transforms.
        foreach (var t in kids)
            t.SetParent(parent, true); // true => keep world pose

        // Remove the temporary parent.
        Object.DestroyImmediate(root.gameObject);
    }
}