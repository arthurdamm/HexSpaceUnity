using UnityEngine;

namespace HexSpace.Utils
{
    public static class HexMath
    {
        public static Vector3 AxialToWorld(int q, int r, float hexSize)
        {
            return AxialPointyTopToWorld(q, r, hexSize);
        }

        public static Vector2Int WorldToAxial(Vector3 worldPos, float hexSize)
        {
            return WorldToAxialPointyTop(worldPos, hexSize);
        }

        public static Vector3 AxialFlatTopToWorld(int q, int r, float hexSize)
        {
            float x = hexSize * 3f / 2f * q;
            float z = hexSize * Mathf.Sqrt(3f) * (r + q / 2f);
            return new Vector3(x, 0, z);
        }

        public static Vector3 AxialPointyTopToWorld(int q, int r, float hexSize)
        {
            float x = hexSize * Mathf.Sqrt(3f) * (q + r / 2f);
            float z = hexSize * 3f / 2f * -r;
            return new Vector3(x, 0, z);
        }

        public static Vector2Int WorldToAxialPointyTop(Vector3 worldPos, float hexSize)
        {
            float q = (Mathf.Sqrt(3f) / 3f * worldPos.x - 1f / 3f * -worldPos.z) / hexSize;
            float r = (2f / 3f * -worldPos.z) / hexSize;

            return AxialPointyTopRoundBranchless(q, r);
        }

        public static Vector2Int AxialPointyTopRoundBranchless(float x, float y)
        {
            float xgrid = Mathf.Round(x);
            float ygrid = Mathf.Round(y);

            x -= xgrid;
            y -= ygrid;
            float dx = Mathf.Round(x + 0.5f * y) * ((y * y) <= (x * x) ? 1 : 0);
            float dy = Mathf.Round(y + 0.5f * x) * ((x * x) <= (y * y) ? 1 : 0);
            xgrid += dx;
            ygrid += dy;

            int finalX = Mathf.RoundToInt(xgrid);
            int finalY = Mathf.RoundToInt(ygrid);
            return new Vector2Int(finalX, finalY);
        }
    }
}