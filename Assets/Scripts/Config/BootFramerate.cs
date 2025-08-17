using UnityEngine;

public class BootFramerate : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Debug.Log("BootFramerate::Init()");
        // vSync is ignored on Android; still set it to be explicit everywhere else
        QualitySettings.vSyncCount = 1;

        // Ask for 60 or higher. Use 120 if your device supports it.
        Application.targetFrameRate = 120;

        // Optional: prevent device from sleeping while you test
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
