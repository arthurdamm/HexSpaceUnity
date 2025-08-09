using UnityEngine;
using System;


public static class HexSettings
{
    // Change if you want a different path: Assets/Resources/HexConfig.asset
    private const string ConfigPath = "HexConfig";

    private static HexConfig _config;

    /// <summary>
    /// Subscribe to know when the active config changes (via Bind/Reload).
    /// </summary>
    public static event Action OnConfigChanged;

    /// <summary>
    /// Active config (lazy-loaded). Will try Resources/HexConfig first.
    /// </summary>
    public static HexConfig Config
    {
        get
        {
            if (_config == null)
            {
                _config = Resources.Load<HexConfig>(ConfigPath);
#if UNITY_EDITOR
                if (_config == null)
                    Debug.LogError($"HexSettings: Missing Resources/{ConfigPath}.asset");
#endif
            }
            return _config;
        }
    }

    // Shorthand properties
    public static float HexSize => Config != null ? Config.hexSize : 1f;
    public static int GridRadius => Config != null ? Config.gridRadius : 5;

    /// <summary>
    /// Bind a config at runtime (editor tool, presets, tests).
    /// </summary>
    public static void Bind(HexConfig cfg)
    {
        if (_config == cfg) return;
        _config = cfg;
        OnConfigChanged?.Invoke();
    }

    /// <summary>
    /// Force reload from Resources (e.g., after you swapped the asset on disk).
    /// </summary>
    public static void Reload()
    {
        _config = null;
        _ = Config; // trigger lazy load
        OnConfigChanged?.Invoke();
    }
}

// public class HexSettings : MonoBehaviour
// {
//     public static HexSettings I { get; private set; }

//     private const string ConfigPath = "HexConfig"; // Assets/Resources/HexConfig.asset
//     private static HexConfig _cfg;                 // <-- you were missing this

//     // Safe, lazy size getter (works in edit/play; no scene order issues)
//     private static float Size
//     {
//         get
//         {
//             if (_cfg == null)
//             {
//                 _cfg = Resources.Load<HexConfig>(ConfigPath);
// #if UNITY_EDITOR
//                 if (_cfg == null)
//                     Debug.LogError($"GameHex: Missing Resources/{ConfigPath}.asset");
// #endif
//             }
//             return _cfg != null ? _cfg.hexSize : 1f; // last-resort default
//         }
//     }

//     public float HexSize => _cfg.hexSize;
//     public int GridRadius => _cfg.gridRadius;

//     public event Action OnConfigChanged;

//     private void Awake()
//     {
//         if (I != null && I != this) { Destroy(gameObject); return; }
//         I = this;
//         DontDestroyOnLoad(gameObject);
//     }

//     // Optional: hot-swap configs at runtime (editor tools, presets, etc.)
//     public void ApplyConfig(HexConfig newConfig)
//     {
//         if (newConfig == null || newConfig == _cfg) return;
//         _cfg = newConfig;
//         OnConfigChanged?.Invoke();
//     }
// }
