using UnityEngine;

/// <summary>
/// Interface for the movement system that handles pathfinding and grid logic
/// </summary>
public interface IMovementSystem
{
    /// <summary>
    /// Calculate the shortest path between two positions
    /// Returns null if no path exists
    /// </summary>
    Vector2Int[] FindPath(Vector2Int start, Vector2Int end, IMovable unit);
    
    /// <summary>
    /// Get the movement cost between two adjacent hexes
    /// </summary>
    int GetMovementCost(Vector2Int from, Vector2Int to, IMovable unit);
    
    /// <summary>
    /// Check if a position is valid for movement (not blocked, in bounds, etc.)
    /// </summary>
    bool IsValidPosition(Vector2Int position, IMovable unit);
    
    /// <summary>
    /// Get all positions within movement range of a unit
    /// </summary>
    Vector2Int[] GetPositionsInRange(Vector2Int start, int movementPoints, IMovable unit);
    
    /// <summary>
    /// Check if there's a unit at the specified position
    /// </summary>
    bool IsPositionOccupied(Vector2Int position);
    
    /// <summary>
    /// Get the unit at the specified position (if any)
    /// </summary>
    IMovable GetUnitAt(Vector2Int position);
}
