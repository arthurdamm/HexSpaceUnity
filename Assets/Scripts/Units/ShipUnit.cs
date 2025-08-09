using UnityEngine;

public class ShipUnit : MonoBehaviour, ISelectable, IMovable
{
    public string ShipName = "ShipUnit";
    public int OwnerID;
    
    [SerializeField] private Vector2Int axialPosition;
    [SerializeField] private int maxMovementPoints = 3;
    [SerializeField] private int currentMovementPoints = 3;
    
    // Reference to movement system (inject via dependency injection or find in scene)
    private IMovementSystem movementSystem;
    
    void Start()
    {
        // Find movement system in scene or inject it
        movementSystem = FindObjectOfType<MovementSystem>(); // You'll implement this
        currentMovementPoints = maxMovementPoints;
    }

    #region IMovable Implementation
    
    public Vector2Int Position 
    { 
        get => axialPosition; 
        private set => axialPosition = value; 
    }
    
    public int CurrentMovementPoints => currentMovementPoints;
    public int MaxMovementPoints => maxMovementPoints;

    public bool CanMoveTo(Vector2Int targetPosition)
    {
        if (movementSystem == null) return false;
        
        int cost = GetMovementCost(targetPosition);
        return cost > 0 && cost <= currentMovementPoints && 
               movementSystem.IsValidPosition(targetPosition, this);
    }

    public int GetMovementCost(Vector2Int targetPosition)
    {
        if (movementSystem == null) return -1;
        
        var path = movementSystem.FindPath(Position, targetPosition, this);
        return path?.Length - 1 ?? -1; // -1 because path includes start position
    }

    public bool MoveTo(Vector2Int targetPosition)
    {
        if (!CanMoveTo(targetPosition)) return false;
        
        int cost = GetMovementCost(targetPosition);
        currentMovementPoints -= cost;
        
        Vector2Int oldPosition = Position;
        Position = targetPosition;
        
        Debug.Log($"{ShipName} moved from {oldPosition} to {targetPosition}. Movement points: {currentMovementPoints}");
        
        // Update visual position here
        UpdateVisualPosition();
        
        return true;
    }

    public void RefreshMovementPoints()
    {
        currentMovementPoints = maxMovementPoints;
        Debug.Log($"{ShipName} movement points refreshed to {currentMovementPoints}");
    }

    public Vector2Int[] GetValidMovePositions()
    {
        if (movementSystem == null) return new Vector2Int[0];
        
        return movementSystem.GetPositionsInRange(Position, currentMovementPoints, this);
    }
    
    #endregion

    private void UpdateVisualPosition()
    {
        // TODO: Update the actual 3D position based on hex grid
        // This would typically get the world position from your HexInstancer
        // and smoothly move the transform to that position
    }

    public Vector2Int Axial; // Assuming hex grid uses axial coords
    public int MovementPoints = 3;

    #region ISelectable Implementation

    public string GetSelectableName()
    {
        return ShipName;
    }

    public bool OnDeselected()
    {
        Debug.Log($"Ship {ShipName} deselected!");
        return true;
    }

    public bool OnSelected(in SelectionArgs args)
    {
        Debug.Log($"Ship {ShipName} selected!");
        return true;
    }
    
    #endregion
}