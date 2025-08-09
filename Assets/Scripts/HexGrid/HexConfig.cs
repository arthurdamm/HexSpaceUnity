using UnityEngine;

[CreateAssetMenu(fileName = "HexConfig", menuName = "HexSpace/HexConfig")]
public class HexConfig : ScriptableObject
{
    public float hexSize = 1f;
    public int gridRadius = 5;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Called whenever you change fields in the Inspector
        HexSettings.Bind(this);              // update the active reference
        // HexSettings.OnConfigChanged?.Invoke(); // notify listeners
        // (Bind already raises OnConfigChanged if reference changes;
        // this extra Invoke handles "same asset, updated fields".)
    }
#endif
}
