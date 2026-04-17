using System.Runtime.InteropServices;
using UnityEngine;

public static class iOSCoreHaptics
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void StartHapticVibration(float duration);
#else
    private static void StartHapticVibration(float duration) { }
#endif

    public static void Vibrate(float durationInSeconds = 0.5f)
    {
        StartHapticVibration(durationInSeconds);
    }

}