using UnityEngine;

[CreateAssetMenu(fileName = "HexConfig", menuName = "Scriptable Objects/HexConfig")]
public class HexConfig : ScriptableObject
{
    public float hexSize = 1f;
    public int gridRadius = 5;
}
