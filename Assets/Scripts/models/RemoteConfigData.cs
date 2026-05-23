
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public enum RemoteConfigProperty
{
    Undefined = 0,
}

[Serializable]
public class RemoteConfigEntry
{
    public int value;
    public string stringValue;
    public RemoteConfigProperty name;
}

[Serializable]
public class RemoteConfigData
{
    private const string PPrefsKey = "RemoteConfigData";

    public List<RemoteConfigEntry> entries = new List<RemoteConfigEntry>();
    public int GetValue(RemoteConfigProperty p, int fallback)
    {
        var v = entries.Find(e => e.name == p);
        return v == null ? fallback : v.value;
    }

    public string GetValue(RemoteConfigProperty p, string fallback)
    {
        var v = entries.Find(e => e.name == p);
        return v == null ? fallback : v.stringValue;
    }

    public void SetValue(RemoteConfigProperty p, int value)
    {
        var entry = entries.Find(e => e.name == p);
        if (entry != null)
        {
            entry.value = value;
        }
        else
        {
            entries.Add(new RemoteConfigEntry { name = p, value = value });
        }
    }
    //public int DailyCoins => GetValue(RemoteConfigProperty.DailyCoins, 1000);


    public static RemoteConfigData Load()
    {
        try
        {
            if (!FilePrefs.HasKey(PPrefsKey))
            {
                return new RemoteConfigData();
            }
            var json = FilePrefs.GetString(PPrefsKey, "{}");
            var d = JsonUtility.FromJson<RemoteConfigData>(json);
            if (d.entries == null)
            {
                d.entries = new List<RemoteConfigEntry>();
            }
            return d;
        }
        catch (Exception e)
        {
            Debug.LogError("RemoteConfigData Failed to load RemoteConfigData: " + e.Message);
            return new RemoteConfigData();
        }
    }
    public static bool HasSavedData()
    {
        return FilePrefs.HasKey(PPrefsKey);
    }

    public void Save()
    {
        try
        {
            var json = JsonUtility.ToJson(this);
            FilePrefs.SetString(PPrefsKey, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save RemoteConfigData: " + e.Message);
        }
    }
}