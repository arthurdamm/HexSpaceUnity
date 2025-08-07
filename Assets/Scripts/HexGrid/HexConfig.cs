using UnityEngine;

[CreateAssetMenu(fileName = "HexConfig", menuName = "HexSpace/HexConfig")]
public class HexConfig : ScriptableObject
{
    public float hexSize = 1f;
    public int gridRadius = 5;
}
