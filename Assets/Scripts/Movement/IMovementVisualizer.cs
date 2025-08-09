using UnityEngine;

/// <summary>
/// Interface for handling movement animations and visual feedback
/// </summary>
public interface IMovementVisualizer
{
    /// <summary>
    /// Animate a unit moving from one position to another
    /// </summary>
    void AnimateMovement(IMovable unit, Vector2Int from, Vector2Int to, float duration = 1f);
    
    /// <summary>
    /// Show valid movement positions for a unit
    /// </summary>
    void ShowMovementRange(IMovable unit, Vector2Int[] validPositions);
    
    /// <summary>
    /// Hide movement range indicators
    /// </summary>
    void HideMovementRange();
    
    /// <summary>
    /// Show a path from current position to target
    /// </summary>
    void ShowMovementPath(Vector2Int[] path);
    
    /// <summary>
    /// Hide movement path indicators
    /// </summary>
    void HideMovementPath();
    
    /// <summary>
    /// Check if movement animation is currently playing
    /// </summary>
    bool IsAnimating { get; }
}
