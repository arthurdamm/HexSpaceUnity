using UnityEngine;

public class ConfigTest: MonoBehaviour
{
    [Header("Hex Grid Config")]
    public float hexSize = 1f;
    public int gridRadius = 55;

    public static ConfigTest Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
}
