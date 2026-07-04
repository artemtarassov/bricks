using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine.UnityConsent;
using UnityEngine;

public class UnityAnalyticsCache
{
    private List<Unity.Services.Analytics.Event> cachedEvents = new List<Unity.Services.Analytics.Event>();

    public UnityAnalyticsCache()
    {

    }

    public void Send(AdImpressionEvent e)
    {
        cachedEvents.Add(e);
        SendCachedEvents();
    } 

    public void Send(string logEventName, string key, int value)
    {
        var e = new CustomEvent(logEventName.ToString());
        e.Add(key, value);
        cachedEvents.Add(e);
        SendCachedEvents();
    }

    public void Send(string logEventName, string key, string value)
    {
        var e = new CustomEvent(logEventName.ToString());
        e.Add(key, value);
        cachedEvents.Add(e);
        SendCachedEvents();
    }

    public void Send(string logEventName, Dictionary<string, object> dict)
    {
        var e = new CustomEvent(logEventName.ToString());
        foreach (var key in dict.Keys)
        {
            e.Add(key, dict[key]);
        }
        cachedEvents.Add(e);
        SendCachedEvents();
    }
    public void SendCachedEvents()
    {
        if (cachedEvents.Count == 0)
        {
            return;
        }

        if (UnityServices.State != ServicesInitializationState.Initialized || !InitializeUnityServices.AnalyticsCollectionReady)
        {
            return;
        }

        if (EndUserConsent.GetConsentState().AnalyticsIntent != ConsentStatus.Granted)
        {
            return;
        }

        var unsentEvents = new List<Unity.Services.Analytics.Event>();
        foreach (var e in cachedEvents)
        {
            try
            {
                AnalyticsService.Instance.RecordEvent(e);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("UnityAnalyticsCache.SendCachedEvents: " + ex.Message);
                unsentEvents.Add(e);
            }
        }

        cachedEvents = unsentEvents;
    }

}
