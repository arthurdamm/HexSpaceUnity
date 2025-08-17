using UnityEngine;

public enum HexDirection
{
    East = 0,       // X+ direction (0 degrees)
    Northeast = 1,  // 60 degrees
    Northwest = 2,  // 120 degrees  
    West = 3,       // 180 degrees
    Southwest = 4,  // 240 degrees
    Southeast = 5   // 300 degrees
}

public static class HexDirectionExtensions
{
    /// <summary>
    /// Get the axial coordinate offset for moving one step in the given direction
    /// </summary>
    public static Vector2Int GetAxialOffset(this HexDirection direction)
    {
        return direction switch
        {
            HexDirection.East => new Vector2Int(1, 0),
            HexDirection.Northeast => new Vector2Int(1, -1),
            HexDirection.Northwest => new Vector2Int(0, -1),
            HexDirection.West => new Vector2Int(-1, 0),
            HexDirection.Southwest => new Vector2Int(-1, 1),
            HexDirection.Southeast => new Vector2Int(0, 1),
            _ => Vector2Int.zero
        };
    }

    /// <summary>
    /// Get the world space direction vector (normalized) for this hex direction
    /// </summary>
    public static Vector3 GetWorldDirection(this HexDirection direction)
    {
        float angleDegrees = (int)direction * 60f;
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRadians), 0f, -Mathf.Sin(angleRadians));
    }

    /// <summary>
    /// Get the rotation (around Y-axis) that faces this hex direction
    /// </summary>
    public static Quaternion GetRotation(this HexDirection direction)
    {
        float angleDegrees = (int)direction * 60f;
        return Quaternion.Euler(0f, -angleDegrees, 0f);
    }

    /// <summary>
    /// Get the opposite direction
    /// </summary>
    public static HexDirection GetOpposite(this HexDirection direction)
    {
        return (HexDirection)(((int)direction + 3) % 6);
    }

    /// <summary>
    /// Rotate clockwise by the specified number of steps (1 step = 60 degrees)
    /// </summary>
    public static HexDirection RotateClockwise(this HexDirection direction, int steps = 1)
    {
        return (HexDirection)(((int)direction + steps) % 6);
    }

    /// <summary>
    /// Rotate counter-clockwise by the specified number of steps
    /// </summary>
    public static HexDirection RotateCounterClockwise(this HexDirection direction, int steps = 1)
    {
        return (HexDirection)(((int)direction - steps + 6) % 6);
    }

    /// <summary>
    /// Convert a world direction vector to the closest hex direction.
    /// </summary>
    public static HexDirection WorldVectorToHexDirection(Vector3 worldDirection)
    {
        worldDirection.y = 0f; // Project to XZ plane
        worldDirection.Normalize();

        float angle = Mathf.Atan2(worldDirection.z, worldDirection.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        // Round to nearest 60-degree increment
        int directionIndex = Mathf.RoundToInt(angle / 60f) % 6;
        return (HexDirection)directionIndex;
    }

    /// <summary>
    /// Calculate the direction from one hex to an adjacent hex
    /// </summary>
    public static HexDirection GetDirectionTo(Vector2Int fromAxial, Vector2Int toAxial)
    {
        Vector2Int delta = toAxial - fromAxial;
        
        // Check each direction's offset
        for (int i = 0; i < 6; i++)
        {
            HexDirection dir = (HexDirection)i;
            if (dir.GetAxialOffset() == delta)
                return dir;
        }
        
        // Fallback: not adjacent, return East
        return HexDirection.East;
    }
}
