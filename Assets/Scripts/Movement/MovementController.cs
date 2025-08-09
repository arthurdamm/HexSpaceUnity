using UnityEngine;

/// <summary>
/// Controls movement interactions and integrates with the selection system
/// </summary>
public class MovementController : MonoBehaviour, IMovementController
{
    [SerializeField] private IMovementSystem movementSystem;
    [SerializeField] private IMovementVisualizer movementVisualizer;
    
    private IMovable activeUnit;
    private bool isMovementActive;
    
    // Events
    public System.Action<IMovable> OnMovementStarted { get; set; }
    public System.Action<IMovable, Vector2Int, Vector2Int> OnMovementCompleted { get; set; }
    public System.Action OnMovementCancelled { get; set; }
    
    public IMovable ActiveUnit => activeUnit;
    public bool IsMovementActive => isMovementActive;
    
    void Start()
    {
        // Find dependencies if not assigned
        if (movementSystem == null)
            movementSystem = FindObjectOfType<MovementSystem>();
        if (movementVisualizer == null)
            movementVisualizer = FindObjectOfType<MovementVisualizer>();
            
        // Subscribe to selection events
        // SelectionManager.OnSelectionChanged += OnSelectionChanged;
    }
    
    void OnDestroy()
    {
        // SelectionManager.OnSelectionChanged -= OnSelectionChanged;
    }
    
    void Update()
    {
        // Handle right-click to cancel movement
        if (isMovementActive && Input.GetMouseButtonDown(1))
        {
            CancelMovement();
        }
        
        // Handle movement input when in movement mode
        if (isMovementActive && Input.GetMouseButtonDown(0))
        {
            HandleMovementInput();
        }
    }
    
    public void StartMovement(IMovable unit)
    {
        if (unit == null || movementVisualizer?.IsAnimating == true) return;
        
        activeUnit = unit;
        isMovementActive = true;
        
        // Show valid movement positions
        var validPositions = unit.GetValidMovePositions();
        movementVisualizer?.ShowMovementRange(unit, validPositions);
        
        OnMovementStarted?.Invoke(unit);
        // Debug.Log($"Movement started for {unit.GetSelectableName()}");
    }
    
    public void CancelMovement()
    {
        if (!isMovementActive) return;
        
        isMovementActive = false;
        movementVisualizer?.HideMovementRange();
        movementVisualizer?.HideMovementPath();
        
        OnMovementCancelled?.Invoke();
        Debug.Log("Movement cancelled");
        
        activeUnit = null;
    }
    
    public bool ExecuteMovement(Vector2Int targetPosition)
    {
        if (!isMovementActive || activeUnit == null) return false;
        
        Vector2Int oldPosition = activeUnit.Position;
        
        if (activeUnit.MoveTo(targetPosition))
        {
            // Update movement system's unit tracking
            if (movementSystem is MovementSystem ms)
            {
                ms.UpdateUnitPosition(activeUnit, oldPosition, targetPosition);
            }
            
            OnMovementCompleted?.Invoke(activeUnit, oldPosition, targetPosition);
            
            // Cancel movement mode after successful move
            CancelMovement();
            return true;
        }
        
        return false;
    }
    
    private void OnSelectionChanged(ISelectable newSelection, ISelectable oldSelection)
    {
        // Cancel any active movement when selection changes
        if (isMovementActive)
        {
            CancelMovement();
        }
        
        // If new selection is a movable unit, potentially start movement mode
        if (newSelection is IMovable movableUnit)
        {
            // You might want to add a condition here, like double-click or specific key
            // For now, just show that it's ready for movement
            // Debug.Log($"Selected movable unit: {movableUnit.GetSelectableName()}. Press M to start movement.");
        }
    }
    
    private void HandleMovementInput()
    {
        // Cast ray to get target position
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Convert hit point to hex coordinates
            Vector2Int targetAxial = GameHex.WorldToAxial(hit.point);
            
            // Show path preview
            if (movementSystem != null)
            {
                var path = movementSystem.FindPath(activeUnit.Position, targetAxial, activeUnit);
                if (path != null)
                {
                    movementVisualizer?.ShowMovementPath(path);
                }
            }
            
            // Execute movement if valid
            if (activeUnit.CanMoveTo(targetAxial))
            {
                ExecuteMovement(targetAxial);
            }
            else
            {
                Debug.Log("Cannot move to that position");
            }
        }
    }
    
    // Public method to trigger movement mode (can be called from UI or key input)
    public void ToggleMovementMode()
    {
        if (isMovementActive)
        {
            CancelMovement();
        }
        else if (SelectionManager.I.Current is IMovable movableUnit)
        {
            StartMovement(movableUnit);
        }
    }
}
