using UnityEngine;

public static class VibrationManager
{
    public static bool IsVibrate;

    public static void Vibrate()
    {
        if (IsVibrate) return;

#if UNITY_ANDROID
        VibrateAndroid(500); // 500 ms
#elif UNITY_IOS
        VibrateIOS(0.5f); // limited control on iOS
#else
        Handheld.Vibrate(); // default
#endif
    }

#if UNITY_ANDROID
    private static void VibrateAndroid(long milliseconds)
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            if (vibrator != null)
            {
                vibrator.Call("vibrate", milliseconds);
            }
        }
    }
#endif

    public static void VibrateIOS(float duration)
    {
        if (IsCoreHapticsSupported())
            iOSCoreHaptics.Vibrate(duration);
        else
            Handheld.Vibrate(); // basic fallback
    }

    private static bool IsCoreHapticsSupported()
    {
#if UNITY_IOS && !UNITY_EDITOR
    var version = new System.Version(UnityEngine.iOS.Device.systemVersion);
    return version.Major >= 13;
#else
        return false;
#endif
    }


}
