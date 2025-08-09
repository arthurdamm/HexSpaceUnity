using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic implementation of the movement system for hex grid
/// </summary>
public class MovementSystem : MonoBehaviour, IMovementSystem
{
    [SerializeField] private HexInstancer hexGrid;
    [SerializeField] private int defaultMovementCost = 1;
    
    // Track units on the grid
    private Dictionary<Vector2Int, IMovable> unitPositions = new Dictionary<Vector2Int, IMovable>();
    
    void Start()
    {
        if (hexGrid == null)
            hexGrid = FindObjectOfType<HexInstancer>();
    }
    
    public Vector2Int[] FindPath(Vector2Int start, Vector2Int end, IMovable unit)
    {
        // Simple A* pathfinding implementation
        // For now, return direct path if valid, null otherwise
        if (!IsValidPosition(end, unit) || IsPositionOccupied(end))
            return null;
            
        // Calculate direct distance (simplified)
        int distance = GetHexDistance(start, end);
        
        // For now, just return a straight line path
        // In a full implementation, you'd use proper A* pathfinding
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(start);
        
        // Simple direct path (you'd replace this with proper pathfinding)
        Vector2Int current = start;
        while (current != end)
        {
            Vector2Int direction = GetDirectionTowards(current, end);
            current += direction;
            path.Add(current);
        }
        
        return path.ToArray();
    }
    
    public int GetMovementCost(Vector2Int from, Vector2Int to, IMovable unit)
    {
        // Different terrain types could have different costs
        // For now, all movement costs the same
        return defaultMovementCost;
    }
    
    public bool IsValidPosition(Vector2Int position, IMovable unit)
    {
        // Check if position is within grid bounds
        if (hexGrid != null && !hexGrid.TryGetWorldPosition(position, out _))
            return false;
            
        // Check if position is blocked by terrain or other obstacles
        // Add your terrain/obstacle checking logic here
        
        return true;
    }
    
    public Vector2Int[] GetPositionsInRange(Vector2Int start, int movementPoints, IMovable unit)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        // Simple flood fill to find all reachable positions
        Queue<(Vector2Int pos, int remainingPoints)> queue = new Queue<(Vector2Int, int)>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue((start, movementPoints));
        visited.Add(start);
        
        while (queue.Count > 0)
        {
            var (currentPos, remainingPoints) = queue.Dequeue();
            
            // Get all 6 hex neighbors
            Vector2Int[] neighbors = GetHexNeighbors(currentPos);
            
            foreach (var neighbor in neighbors)
            {
                if (visited.Contains(neighbor)) continue;
                
                int cost = GetMovementCost(currentPos, neighbor, unit);
                if (cost <= remainingPoints && IsValidPosition(neighbor, unit) && !IsPositionOccupied(neighbor))
                {
                    visited.Add(neighbor);
                    positions.Add(neighbor);
                    
                    if (remainingPoints - cost > 0)
                    {
                        queue.Enqueue((neighbor, remainingPoints - cost));
                    }
                }
            }
        }
        
        return positions.ToArray();
    }
    
    public bool IsPositionOccupied(Vector2Int position)
    {
        return unitPositions.ContainsKey(position);
    }
    
    public IMovable GetUnitAt(Vector2Int position)
    {
        unitPositions.TryGetValue(position, out IMovable unit);
        return unit;
    }
    
    // Helper methods for hex grid calculations
    private Vector2Int[] GetHexNeighbors(Vector2Int axial)
    {
        // Axial coordinate hex neighbors
        return new Vector2Int[]
        {
            axial + new Vector2Int(1, 0),   // East
            axial + new Vector2Int(1, -1),  // Northeast
            axial + new Vector2Int(0, -1),  // Northwest
            axial + new Vector2Int(-1, 0),  // West
            axial + new Vector2Int(-1, 1),  // Southwest
            axial + new Vector2Int(0, 1)    // Southeast
        };
    }
    
    private int GetHexDistance(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.x + a.y - b.x - b.y) + Mathf.Abs(a.y - b.y)) / 2;
    }
    
    private Vector2Int GetDirectionTowards(Vector2Int from, Vector2Int to)
    {
        Vector2Int diff = to - from;
        
        // Normalize to hex direction (simplified)
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            return new Vector2Int(diff.x > 0 ? 1 : -1, 0);
        else
            return new Vector2Int(0, diff.y > 0 ? 1 : -1);
    }
    
    // Public methods for managing unit positions
    public void RegisterUnit(IMovable unit)
    {
        unitPositions[unit.Position] = unit;
    }
    
    public void UnregisterUnit(IMovable unit)
    {
        unitPositions.Remove(unit.Position);
    }
    
    public void UpdateUnitPosition(IMovable unit, Vector2Int oldPosition, Vector2Int newPosition)
    {
        unitPositions.Remove(oldPosition);
        unitPositions[newPosition] = unit;
    }
}
