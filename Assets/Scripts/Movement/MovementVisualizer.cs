using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles visual feedback for movement system
/// </summary>
public class MovementVisualizer : MonoBehaviour, IMovementVisualizer
{
    [Header("Visual Indicators")]
    [SerializeField] private GameObject movementRangeIndicatorPrefab;
    [SerializeField] private GameObject movementPathIndicatorPrefab;
    [SerializeField] private Material validMoveMaterial;
    [SerializeField] private Material invalidMoveMaterial;
    [SerializeField] private Material pathMaterial;
    
    [Header("Animation Settings")]
    [SerializeField] private float movementDuration = 1f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private List<GameObject> rangeIndicators = new List<GameObject>();
    private List<GameObject> pathIndicators = new List<GameObject>();
    private bool isAnimating = false;
    
    [SerializeField] private HexInstancer hexGrid;
    
    public bool IsAnimating => isAnimating;
    
    void Start()
    {
        if (hexGrid == null)
            hexGrid = FindObjectOfType<HexInstancer>();
    }
    
    public void AnimateMovement(IMovable unit, Vector2Int from, Vector2Int to, float duration = 1f)
    {
        if (isAnimating) return;
        
        StartCoroutine(AnimateMovementCoroutine(unit, from, to, duration));
    }
    
    public void ShowMovementRange(IMovable unit, Vector2Int[] validPositions)
    {
        HideMovementRange(); // Clear any existing indicators
        
        foreach (var position in validPositions)
        {
            if (hexGrid.TryGetWorldPosition(position, out Vector3 worldPos))
            {
                GameObject indicator = CreateRangeIndicator(worldPos, true);
                rangeIndicators.Add(indicator);
            }
        }
    }
    
    public void HideMovementRange()
    {
        foreach (var indicator in rangeIndicators)
        {
            if (indicator != null)
                DestroyImmediate(indicator);
        }
        rangeIndicators.Clear();
    }
    
    public void ShowMovementPath(Vector2Int[] path)
    {
        HideMovementPath(); // Clear any existing path
        
        for (int i = 1; i < path.Length; i++) // Skip the starting position
        {
            if (hexGrid.TryGetWorldPosition(path[i], out Vector3 worldPos))
            {
                GameObject indicator = CreatePathIndicator(worldPos, i - 1);
                pathIndicators.Add(indicator);
            }
        }
    }
    
    public void HideMovementPath()
    {
        foreach (var indicator in pathIndicators)
        {
            if (indicator != null)
                DestroyImmediate(indicator);
        }
        pathIndicators.Clear();
    }
    
    private GameObject CreateRangeIndicator(Vector3 worldPosition, bool isValid)
    {
        GameObject indicator;
        
        if (movementRangeIndicatorPrefab != null)
        {
            indicator = Instantiate(movementRangeIndicatorPrefab, worldPosition, Quaternion.identity);
        }
        else
        {
            // Create simple primitive if no prefab is assigned
            indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.transform.position = worldPosition + Vector3.up * 0.1f;
            indicator.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
            
            // Remove collider to avoid interference
            if (indicator.GetComponent<Collider>())
                DestroyImmediate(indicator.GetComponent<Collider>());
        }
        
        // Apply material
        var renderer = indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = isValid ? validMoveMaterial : invalidMoveMaterial;
        }
        
        return indicator;
    }
    
    private GameObject CreatePathIndicator(Vector3 worldPosition, int pathIndex)
    {
        GameObject indicator;
        
        if (movementPathIndicatorPrefab != null)
        {
            indicator = Instantiate(movementPathIndicatorPrefab, worldPosition, Quaternion.identity);
        }
        else
        {
            // Create simple primitive if no prefab is assigned
            indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.position = worldPosition + Vector3.up * 0.2f;
            indicator.transform.localScale = Vector3.one * 0.3f;
            
            // Remove collider to avoid interference
            if (indicator.GetComponent<Collider>())
                DestroyImmediate(indicator.GetComponent<Collider>());
        }
        
        // Apply material
        var renderer = indicator.GetComponent<Renderer>();
        if (renderer != null && pathMaterial != null)
        {
            renderer.material = pathMaterial;
        }
        
        return indicator;
    }
    
    private IEnumerator AnimateMovementCoroutine(IMovable unit, Vector2Int from, Vector2Int to, float duration)
    {
        isAnimating = true;
        
        // Get world positions
        bool hasFromPos = hexGrid.TryGetWorldPosition(from, out Vector3 fromWorld);
        bool hasToPos = hexGrid.TryGetWorldPosition(to, out Vector3 toWorld);
        
        if (!hasFromPos || !hasToPos)
        {
            isAnimating = false;
            yield break;
        }
        
        // Get the unit's transform (assuming it's a MonoBehaviour)
        Transform unitTransform = null;
        if (unit is MonoBehaviour unitMono)
        {
            unitTransform = unitMono.transform;
        }
        
        if (unitTransform == null)
        {
            isAnimating = false;
            yield break;
        }
        
        float elapsed = 0f;
        Vector3 startPos = fromWorld;
        Vector3 endPos = toWorld;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = movementCurve.Evaluate(t);
            
            // Lerp position with slight arc
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, curveValue);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 0.5f; // Add slight arc
            
            unitTransform.position = currentPos;
            
            yield return null;
        }
        
        // Ensure final position is exact
        unitTransform.position = endPos;
        isAnimating = false;
    }
}
