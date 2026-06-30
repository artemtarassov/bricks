
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public enum RemoteConfigProperty
{
    Undefined = 0,
    MaxAttempts = 1,
    RefillCoins = 2,
    Reward1Coins = 3,
    AdditionalEmitterSec = 4,
    DailyRewardCoinsGoldenTicket = 5,
    DailyRewardCoinsGoldenTicketTemp = 6,
    CompleteRewardCoins = 7,
    ColumnCoins = 8,
    MusicTrack = 9,
    ShowBannerAfterSec = 10,
    ShowMidSessionAdAfterSec = 11,
    Announcer = 12,
    FillAttemptsAfterRestart = 13,
    FinishElementType = 14,
    CompleteChallengeRewardCoins = 15,
    AddSecondsInChallenge = 16
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
    public int MaxAttempts => GetValue(RemoteConfigProperty.MaxAttempts, 5);
    public int RefillCoins => GetValue(RemoteConfigProperty.RefillCoins, 900);

    //reward for completing one cityElement.
    public int Reward1Coins => GetValue(RemoteConfigProperty.Reward1Coins, 50);
    public int AdditionalEmitterSec => GetValue(RemoteConfigProperty.AdditionalEmitterSec, 5 * 60);

    public int DailyRewardCoinsGoldenTicket => GetValue(RemoteConfigProperty.DailyRewardCoinsGoldenTicket, 2000);
    public int DailyRewardCoinsGoldenTicketTemp => GetValue(RemoteConfigProperty.DailyRewardCoinsGoldenTicketTemp, 1000);

    public int CompleteRewardCoins => GetValue(RemoteConfigProperty.CompleteRewardCoins, 500);
    public int CompleteChallengeRewardCoins => GetValue(RemoteConfigProperty.CompleteChallengeRewardCoins, 100);

    public int ColumnCoins => GetValue(RemoteConfigProperty.ColumnCoins, 50);

    public int MusicTrack => GetValue(RemoteConfigProperty.MusicTrack, 0);//disabled by default.

    public int ShowBannerAfterSec => GetValue(RemoteConfigProperty.ShowBannerAfterSec, 60 * 60 * 24 * 8);//8 days by default.


    public int ShowMidSessionAdAfterSec => GetValue(RemoteConfigProperty.ShowMidSessionAdAfterSec, 60 * 5);

    public bool Announcer => GetValue(RemoteConfigProperty.Announcer, 1) == 1;
    public bool FillAttemptsAfterRestart => GetValue(RemoteConfigProperty.FillAttemptsAfterRestart, 1) == 1;

    public int FinishElementType => GetValue(RemoteConfigProperty.FinishElementType, 0);//0=undefined,SlideDown=100, Explosion=200

    public int AddSecondsInChallenge => GetValue(RemoteConfigProperty.AddSecondsInChallenge, 10);
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