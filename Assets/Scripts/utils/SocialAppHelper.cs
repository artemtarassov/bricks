public static class SocialAppHelper
{
    public static bool HasWhatsApp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AppInstallChecker.IsAppInstalled("com.whatsapp");
#elif UNITY_IOS && !UNITY_EDITOR
        return AppInstallChecker.IsAppInstalled("whatsapp://");
#else
        return false;
#endif
    }

    public static bool HasFacebook()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AppInstallChecker.IsAppInstalled("com.facebook.katana");
#elif UNITY_IOS && !UNITY_EDITOR
        return AppInstallChecker.IsAppInstalled("fbapi://");
#else
        return false;
#endif
    }

    public static bool HasInstagram()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AppInstallChecker.IsAppInstalled("com.instagram.android");
#elif UNITY_IOS && !UNITY_EDITOR
        return AppInstallChecker.IsAppInstalled("instagram://");
#else
        return false;
#endif
    }
}

public static class AppInstallChecker
{
    /// <summary>
    /// Single entry point.
    /// Android: identifier = package name, e.g. "com.whatsapp"
    /// iOS: identifier = URL scheme, e.g. "whatsapp://"
    /// Windows: identifier = friendly name to search in registry, e.g. "Google Chrome"
    /// macOS: identifier = bundle id, e.g. "com.google.Chrome" or an app name fragment like "Google Chrome"
    /// </summary>
    public static bool IsAppInstalled(string identifier)
    {
        bool result = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        result = IsAppInstalled_Android(identifier);
#elif UNITY_IOS && !UNITY_EDITOR
        result = IsAppInstalled_iOS(identifier);
#endif
        return result;
    }

    /// <summary>
    /// Attempts to open the target app right away.
    /// Android: package name
    /// iOS: URL scheme
    /// Windows: .exe path or URL, falls back to ShellExecute
    /// macOS: bundle id preferred, then tries open -b or open on .app
    /// </summary>
    public static bool TryOpenApp(string identifier)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var pm = activity.Call<AndroidJavaObject>("getPackageManager");
                var intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", identifier);
                if (intent == null) return false;
                activity.Call("startActivity", intent);
                return true;
            }
        }
        catch (AndroidJavaException e)
        {
            Debug.LogWarning("TryOpenApp Android failed: " + e.Message);
            return false;
        }
#elif UNITY_IOS && !UNITY_EDITOR
        return _TryOpenURL(identifier);
#endif
      
        return false;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static bool IsAppInstalled_Android(string packageName)
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var pm = activity.Call<AndroidJavaObject>("getPackageManager");
                pm.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
                return true;
            }
        }
        catch (AndroidJavaException)
        {
            return false;
        }
    }
#endif

#if UNITY_IOS && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool _CanOpenURL(string urlScheme);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool _TryOpenURL(string urlScheme);

    private static bool IsAppInstalled_iOS(string urlScheme)
    {
        return _CanOpenURL(urlScheme);
    }
#endif


}
