using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Services.Authentication;
using Unity.Services.Analytics;
using Unity.Services.Core;

public static class UserIDHelper
{
    public static string GetUserId()
    {
        var customLogUserId = FilePrefs.HasKey("customLogUserId") ? FilePrefs.GetString("customLogUserId") : null;
        if (string.IsNullOrEmpty(customLogUserId))
        {
            customLogUserId = System.Guid.NewGuid().ToString();
            FilePrefs.SetString("customLogUserId", customLogUserId);
        }
        return customLogUserId;
    }

    public static string GetAny()
    {
        var analyticsUserId = GetAnalyticsUserId();
        if (!string.IsNullOrEmpty(analyticsUserId))
        {
            return analyticsUserId;
        }
        return GetUserId();
    }

    public static string GetAnalyticsUserId()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    var userId = AnalyticsService.Instance.GetAnalyticsUserID();
                    FilePrefs.SetString("analyticsUserId", userId);
                    return userId;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to get Analytics User ID: {e}");
        }
        if (FilePrefs.HasKey("analyticsUserId"))
        {
            return FilePrefs.GetString("analyticsUserId");
        }
        return "";
    }

}