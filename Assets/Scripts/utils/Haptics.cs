using System;
using UnityEngine;

public static class Haptics
{
    public static void Light()
    {
#if UNITY_IOS && !UNITY_EDITOR
        HapticsIOS.Light();
#elif UNITY_ANDROID && !UNITY_EDITOR
        HapticsAndroid.Light();
#endif
    }

    public static void Medium()
    {
#if UNITY_IOS && !UNITY_EDITOR
        HapticsIOS.Medium();
#elif UNITY_ANDROID && !UNITY_EDITOR
        HapticsAndroid.Medium();
#endif
    }
}

internal static class HapticsIOS
{
    private static bool hasError;
    private static bool isInitialized;

    public static void Light()
    {
        Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactLight);
    }

    public static void Medium()
    {
        Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactMedium);
    }

    private static void EnsureInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        iOSHapticFeedback.Instance.debug = false;
        isInitialized = true;
    }

    private static void Trigger(iOSHapticFeedback.iOSFeedbackType feedbackType)
    {
        if (hasError)
        {
            return;
        }

        try
        {
            EnsureInitialized();
            iOSHapticFeedback.Instance.Trigger(feedbackType);
        }
        catch (Exception e)
        {
            hasError = true;
            Debug.Log("Haptics iOS error: " + e.Message);
        }
    }
}

internal static class HapticsAndroid
{
    private const string UnityPlayerClassName = "com.unity3d.player.UnityPlayer";
    private const string HapticFeedbackConstantsClassName = "android.view.HapticFeedbackConstants";
    private const string BuildVersionClassName = "android.os.Build$VERSION";
    private const string ContextClassName = "android.content.Context";

    private static bool hasError;
    private static bool isInitialized;

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass unityPlayerClass;
    private static bool hasVibrator;
    private static bool supportsSubtleHaptics;
    private static int lightFeedbackConstant;
    private static int mediumFeedbackConstant;
#endif

    public static void Light()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Trigger(lightFeedbackConstant, isSubtle: true);
#endif
    }

    public static void Medium()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Trigger(mediumFeedbackConstant, isSubtle: false);
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void EnsureInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        unityPlayerClass ??= new AndroidJavaClass(UnityPlayerClassName);

        using var constants = new AndroidJavaClass(HapticFeedbackConstantsClassName);
        lightFeedbackConstant = constants.GetStatic<int>("CLOCK_TICK");
        mediumFeedbackConstant = constants.GetStatic<int>("CONTEXT_CLICK");

        using var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        using var contextClass = new AndroidJavaClass(ContextClassName);
        string vibratorService = contextClass.GetStatic<string>("VIBRATOR_SERVICE");
        using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", vibratorService);

        if (vibrator != null)
        {
            hasVibrator = vibrator.Call<bool>("hasVibrator");

            using var buildVersion = new AndroidJavaClass(BuildVersionClassName);
            int sdkInt = buildVersion.GetStatic<int>("SDK_INT");
            supportsSubtleHaptics = hasVibrator && sdkInt >= 26 && vibrator.Call<bool>("hasAmplitudeControl");
        }

        Debug.Log($"Haptics Android initialized. Has vibrator: {hasVibrator}, Supports subtle haptics: {supportsSubtleHaptics}");

        isInitialized = true;
    }

    private static void Trigger(int feedbackConstant, bool isSubtle)
    {
        if (hasError)
        {
            //Debug.Log("Haptics Android: Previous error detected, skipping haptic feedback.");
            return;
        }

        try
        {
            EnsureInitialized();

            if (unityPlayerClass == null || !hasVibrator)
            {
                //Debug.Log("Haptics Android: No vibrator found.");
                return;
            }

            if (isSubtle && !supportsSubtleHaptics)
            {
                //Debug.Log("Haptics Android: Subtle haptics not supported on this device.");
                return;
            }

            using var activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            using var window = activity.Call<AndroidJavaObject>("getWindow");
            using var view = window.Call<AndroidJavaObject>("getDecorView");

            if (!view.Call<bool>("performHapticFeedback", feedbackConstant) && Debug.isDebugBuild)
            {
               /* Debug.Log(
                    isSubtle
                        ? "Haptics Android: light feedback was not performed."
                        : "Haptics Android: medium feedback was not performed.");*/
            } else {
                /*Debug.Log(
                    isSubtle
                        ? "Haptics Android: light feedback performed successfully."
                        : "Haptics Android: medium feedback performed successfully.");*/
            }
        }
        catch (Exception e)
        {
            hasError = true;
            Debug.Log("Haptics Android error: " + e.Message);
        }
    }
#endif
}
