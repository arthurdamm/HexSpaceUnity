using UnityEngine;

/// <summary>
/// Interface for units that can move on the hex grid
/// </summary>
public interface IMovable
{
    /// <summary>
    /// Current position in axial coordinates
    /// </summary>
    Vector2Int Position { get; }
    
    /// <summary>
    /// Current movement points available this turn
    /// </summary>
    int CurrentMovementPoints { get; }
    
    /// <summary>
    /// Maximum movement points per turn
    /// </summary>
    int MaxMovementPoints { get; }
    
    /// <summary>
    /// Check if the unit can move to the specified position
    /// </summary>
    bool CanMoveTo(Vector2Int targetPosition);
    
    /// <summary>
    /// Get the movement cost to reach the target position
    /// </summary>
    int GetMovementCost(Vector2Int targetPosition);
    
    /// <summary>
    /// Execute movement to the target position
    /// Returns true if movement was successful
    /// </summary>
    bool MoveTo(Vector2Int targetPosition);
    
    /// <summary>
    /// Restore movement points (typically called at start of turn)
    /// </summary>
    void RefreshMovementPoints();
    
    /// <summary>
    /// Get all valid positions this unit can move to with current movement points
    /// </summary>
    Vector2Int[] GetValidMovePositions();
}
