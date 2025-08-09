using UnityEngine;

public static class GameHex
{
    private static float Size => HexSettings.HexSize;


    public static Vector3 AxialToWorld(int q, int r)
        => HexSpace.Utils.HexMath.AxialToWorld(q, r, Size);

    public static Vector2Int WorldToAxial(Vector3 worldPos)
        => HexSpace.Utils.HexMath.WorldToAxial(worldPos, Size);

    public static Vector3 AxialFlatTopToWorld(int q, int r)
        => HexSpace.Utils.HexMath.AxialFlatTopToWorld(q, r, Size);

    public static Vector3 AxialPointyTopToWorld(int q, int r)
        => HexSpace.Utils.HexMath.AxialPointyTopToWorld(q, r, Size);

    public static Vector2Int WorldToAxialPointyTop(Vector3 worldPos)
        => HexSpace.Utils.HexMath.WorldToAxialPointyTop(worldPos, Size);
}