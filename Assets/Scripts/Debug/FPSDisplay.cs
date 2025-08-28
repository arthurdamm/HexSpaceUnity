using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    public bool Show = true;

    [Range(0.01f, 1f)] public float smoothing = 0.1f;  // lower = smoother
    public int fontSize = 16;
    public Color textColor = Color.white;
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.35f);

    public Vector2 padding = new Vector2(10, 10);
    public Vector2 margin = new Vector2(10, 10);

    public Corner anchor = Corner.TopLeft;
    public bool showFrameTimeMs = true;
    public bool showVSyncAndTarget = true;
    public KeyCode toggleKey = KeyCode.F1;

    float _delta;            // smoothed unscaled delta time
    GUIStyle _style;
    Texture2D _bgTex;

    public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

    void Awake()
    {
        // prime with current delta so we don’t show huge first-frame spikes
        _delta = Mathf.Max(Time.unscaledDeltaTime, 1e-6f);

        _style = new GUIStyle
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = fontSize,
            normal = { textColor = textColor }
        };

        _bgTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _bgTex.SetPixel(0, 0, backgroundColor);
        _bgTex.Apply();
    }

    void Update()
    {
        if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
            Show = !Show;

        // Exponential moving average (unscaled so it ignores Time.timeScale)
        _delta += (Time.unscaledDeltaTime - _delta) * Mathf.Clamp01(smoothing);
    }

    void OnGUI()
    {
        if (!Show) return;

        // keep style in sync if you tweak in Inspector at runtime
        if (_style.fontSize != fontSize) _style.fontSize = fontSize;
        if (_style.normal.textColor != textColor) _style.normal.textColor = textColor;

        // compose text
        float fps = 1f / Mathf.Max(_delta, 1e-6f);
        float ms = _delta * 1000f;

        string line1 = showFrameTimeMs
            ? $"{ms:0.0} ms  ({fps:0.} fps)"
            : $"{fps:0.} fps";

        string line2 = "";
        if (showVSyncAndTarget)
        {
            int vSync = QualitySettings.vSyncCount;         // 0 = off
            int target = Application.targetFrameRate;        // -1 = platform default
            line2 = $"vSync:{vSync}  target:{(target < 0 ? "default" : target.ToString())}";
        }

        string text = string.IsNullOrEmpty(line2) ? line1 : $"{line1}\n{line2}";

        // Measure and position
        Vector2 size = _style.CalcSize(new GUIContent(text));
        // CalcSize doesn’t account for newlines well; pad vertically for extra lines
        int lines = 1 + (text.Split('\n').Length - 1);
        size.y = (_style.lineHeight <= 0 ? size.y : _style.lineHeight * lines);

        Rect rect = new Rect(0, 0, size.x + padding.x * 2, size.y + padding.y * 2);

        switch (anchor)
        {
            case Corner.TopLeft:
                rect.position = new Vector2(margin.x, margin.y);
                _style.alignment = TextAnchor.UpperLeft;
                break;
            case Corner.TopRight:
                rect.position = new Vector2(Screen.width - rect.width - margin.x, margin.y);
                _style.alignment = TextAnchor.UpperRight;
                break;
            case Corner.BottomLeft:
                rect.position = new Vector2(margin.x, Screen.height - rect.height - margin.y);
                _style.alignment = TextAnchor.LowerLeft;
                break;
            case Corner.BottomRight:
                rect.position = new Vector2(Screen.width - rect.width - margin.x, Screen.height - rect.height - margin.y);
                _style.alignment = TextAnchor.LowerRight;
                break;
        }

        // background box
        var prevBg = GUI.skin.box.normal.background;
        GUI.skin.box.normal.background = _bgTex;
        GUI.Box(rect, GUIContent.none);
        GUI.skin.box.normal.background = prevBg;

        // text
        var contentRect = new Rect(rect.x + padding.x, rect.y + padding.y, rect.width - padding.x * 2, rect.height - padding.y * 2);
        GUI.Label(contentRect, text, _style);
    }

    void OnDestroy()
    {
        if (_bgTex) Destroy(_bgTex);
    }
}