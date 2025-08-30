using Unity.Cinemachine;
using UnityEngine;

public class InputRouter : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask gridLayer;

    [SerializeField] private HexInstancer grid;

    [Tooltip("Click a ship to select; then click a hex to move the selected ship to that hex center.")]
    // [SerializeField] private HexGridController hexGrid;

    void Awake()
    {
        if (mainCamera == null)
            ;
            // mainCamera = Camera.main;
    }


    void OnDrawGizmos()
    {
        Vector2Int[] ring = {
            new(0,0),
            new(1,0), new(1,-1), new(0,-1),
            new(-1,0), new(-1,1), new(0,1)
        };

        foreach (var h in ring)
        {
            var p = GameHex.AxialToWorld(h.x, h.y);
            Gizmos.DrawSphere(p, 0.25f);

            // round-trip check
            var hr = GameHex.WorldToAxial(p);
            Debug.Assert(hr == h, $"Round-trip mismatch: {h} -> {hr}");
        }

        // Direction rays from origin
        for (int i=0;i<6;i++)
        {
            var v = ((HexDirection)i).GetWorldDirection();
            Gizmos.DrawLine(Vector3.zero, v);
        }
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

                // If a ship is selected and we clicked a hex on the grid, issue a move and stop.
                Debug.Log($"LAYER {IsOnLayer(hit.collider.gameObject, gridLayer)}");
                if (axial.HasValue && hexCenter.HasValue && IsOnLayer(hit.collider.gameObject, gridLayer) && SelectionManager.TryGet<ShipUnit>(out var ship))
                {
                    Debug.Log("MOVING SHIP");

                    void OnArrived(ShipUnit u)
                    {
                        ship.Arrived -= OnArrived;
                        SelectionManager.Instance.Clear();
                    }
                    ship.Arrived += OnArrived;

                    ship.CommandMove(axial.Value, hexCenter.Value + Vector3.up * 3);
                    return; // do not let this click fall through to selection
                }

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
    private static bool IsOnLayer(GameObject go, LayerMask mask)
    {
        return (mask.value & (1 << go.layer)) != 0;
    }
}
