using System;
using UnityEngine;

public class InputRouter : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask gridLayer;

    [SerializeField] private HexInstancer grid;

    // [SerializeField] private HexGridController hexGrid;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var world = hit.point;
                var screen = (Vector2)Input.mousePosition;
                var add = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                var alt = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

                Vector2Int? axial = null;
                Vector3? hexCenter = null;
                if (TryResolveHexFromHit(hit, out var hexResult))
                {
                    axial = hexResult.axial;
                    hexCenter = hexResult.hexCenter;
                }

                var args = new SelectionArgs(
                    world, screen, hit, add, alt, mouseButton: 0, mainCamera, axial, hexCenter, source: this);

                if (hit.collider.GetComponentInParent<ISelectable>() is { } selectable)
                {
                    Debug.Log($"FOUND SELECTABLE: {selectable.GetSelectableName()}");
                    SelectionManager.Select(selectable, in args);

                }
                // debugCollider(hit);
                
            }
        }


    }

    private bool TryResolveHexFromHit(RaycastHit hit, out (Vector2Int axial, Vector3 hexCenter) result)
    {
        var axial = GameHex.WorldToAxial(hit.point);
        if (grid.TryGetWorldPosition(axial, out var center))
        {
            result = (axial, center);
            return true;
        }

        result = default; // (= (default, default))
        return false;
    }

    void debugCollider(RaycastHit hit)
    {
         Debug.Log("=== Raycast Hit Info ===");

        // Name of the GameObject the collider is attached to
        Debug.Log("GameObject: " + hit.collider.gameObject.name);

        // Tag (if any)
        Debug.Log("Tag: " + hit.collider.tag);

        // Layer number (integer) and name (string)
        Debug.Log("Layer (int): " + hit.collider.gameObject.layer);
        Debug.Log("Layer (name): " + LayerMask.LayerToName(hit.collider.gameObject.layer));

        // Position where the ray hit
        Debug.Log("Hit Point: " + hit.point);

        // The normal of the surface hit
        Debug.Log("Hit Normal: " + hit.normal);

        // Distance from ray origin to hit point
        Debug.Log("Hit Distance: " + hit.distance);

        // Type of collider
        Debug.Log("Collider Type: " + hit.collider.GetType().Name);

        // If the collider is a child, this will show its parent
        Debug.Log("Parent: " + hit.collider.transform.parent?.name);

    }
}

