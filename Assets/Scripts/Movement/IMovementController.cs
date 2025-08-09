using UnityEngine;

/// <summary>
/// Interface for controlling turn-based movement
/// </summary>
public interface IMovementController
{
    /// <summary>
    /// Current unit that can move (if any)
    /// </summary>
    IMovable ActiveUnit { get; }
    
    /// <summary>
    /// Start movement mode for a specific unit
    /// </summary>
    void StartMovement(IMovable unit);
    
    /// <summary>
    /// Cancel current movement operation
    /// </summary>
    void CancelMovement();
    
    /// <summary>
    /// Execute movement to the target position
    /// </summary>
    bool ExecuteMovement(Vector2Int targetPosition);
    
    /// <summary>
    /// Check if movement mode is currently active
    /// </summary>
    bool IsMovementActive { get; }
    
    /// <summary>
    /// Event triggered when movement starts
    /// </summary>
    System.Action<IMovable> OnMovementStarted { get; set; }
    
    /// <summary>
    /// Event triggered when movement completes
    /// </summary>
    System.Action<IMovable, Vector2Int, Vector2Int> OnMovementCompleted { get; set; }
    
    /// <summary>
    /// Event triggered when movement is cancelled
    /// </summary>
    System.Action OnMovementCancelled { get; set; }
}
