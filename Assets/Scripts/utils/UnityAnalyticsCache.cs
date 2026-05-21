using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class UnityAnalyticsCache
{
    private List<Unity.Services.Analytics.Event> cachedEvents = new List<Unity.Services.Analytics.Event>();

    public void Send(AdImpressionEvent e)
    {
        cachedEvents.Add(e);
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
    private void SendCachedEvents()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            return;
        }
        foreach (var e in cachedEvents)
        {
            try
            {
                AnalyticsService.Instance.RecordEvent(e);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("UnityAnalyticsCache.SendCachedEvents: " + ex.Message);
            }
        }
        cachedEvents.Clear();
    }

}