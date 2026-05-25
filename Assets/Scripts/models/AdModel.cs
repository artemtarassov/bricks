using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
/*

 Admob App ID (tarassov@badmonkee.de account)
   android: c...
   ios: c....
*/

[Serializable]
public enum RewardName
{
    UNDEFINED = 0,
    SPACE1 = 1,
}

[Serializable]
public class AdRewardData
{
    public string adUnit => AdUnits.GetAdUnitForReward(this.rewardName);
    public RewardName rewardName = RewardName.UNDEFINED;
    public int cash = 0;
    public AdRewardData(RewardName rn)
    {
        this.rewardName = rn;
    }
}

[Serializable]
public class AdData
{
    [NonSerialized]
    public bool dirty = false;

    [NonSerialized]
    public List<AdRewardData> requested = new List<AdRewardData>();

    public List<int> earnedRewardDayOfYear = new List<int>();
}

public class AdModel
{
    public static AdModel Instance;

    public Action<string, bool> OnAdReady;
    public Action<AdRewardData> OnAdShow;
    public Action<AdRewardData> OnRewardEarned;
    public Action<AdRewardData> OnRewardFailed;
    public Action<string> OnAdHide;

    private AdData adData;

    private HashSet<string> adUnitsReady = new HashSet<string>();

    private const string savekey = "adsdata";

    public bool shouldLoadAds = true;

    public void LoadData()
    {
        if (FilePrefs.HasKey(savekey))
        {
            try
            {
                adData = JsonUtility.FromJson<AdData>(FilePrefs.GetString(savekey));
            }
            catch (Exception e)
            {
                Debug.LogError("AdModel LoadData: " + e.Message);
                adData = new AdData();
            }
        }
        else
        {
            adData = new AdData();
        }
        if (adData.earnedRewardDayOfYear == null)
        {
            adData.earnedRewardDayOfYear = new List<int>();
        }
        if (adData.requested == null)
        {
            adData.requested = new List<AdRewardData>();
        }
    }

    private float loadErrorTime = 0;

    public void SetLoadError()
    {
        loadErrorTime = Time.time;
    }

    public bool HasLoadError()
    {
        if (loadErrorTime == 0)
        {
            return false;
        }
        return Time.time - loadErrorTime < 4;
    }

    public bool SaveDataIfDirty()
    {
        if (!adData.dirty)
        {
            return false;
        }
        adData.dirty = false;
        FilePrefs.SetString(savekey, JsonUtility.ToJson(adData));
        return true;
    }

    public void HideAd(string adUnit)
    {
        SetAdReady(adUnit, false);
        this.adData.requested.RemoveAll(rd => rd.adUnit == adUnit);
        this.adData.dirty = true;
        OnAdHide?.Invoke(adUnit);
    }

    public bool AllReady()
    {
        var allRewardNames = Enum.GetValues(typeof(RewardName));
        foreach (RewardName rewardName in allRewardNames)
        {
            if (rewardName == RewardName.UNDEFINED)
            {
                continue;
            }
            if (!AdModel.Instance.IsAdReady(rewardName))
            {
                return false;
            }
        }
        return true;
    }

    public void SetAdReady(string adUnit, bool ready)
    {
        if (ready)
        {
            loadErrorTime = 0;
            if (this.adUnitsReady.Contains(adUnit))
            {
                return;
            }
            adUnitsReady.Add(adUnit);
            OnAdReady?.Invoke(adUnit, true);
        }
        else
        {
            if (!this.adUnitsReady.Contains(adUnit))
            {
                return;
            }

            adUnitsReady.Remove(adUnit);
            OnAdReady?.Invoke(adUnit, false);
        }
    }

    public void ShowAd(AdRewardData rd)
    {
        if (!IsAdReady(rd.adUnit.ToString()))
        {
            Debug.LogError("AdModel.Request() - is not ready");
            return;
        }
        this.adData.dirty = true;
        this.adData.requested.Add(rd);
        this.OnAdShow.Invoke(rd);
    }

    public void SetRewardEarned(string adUnit)
    {
        if (string.IsNullOrEmpty(adUnit))
        {
            Debug.LogError("AdModel.SetRewardEarned - adUnit is null or empty");
            return;
        }
        var rewardDataByAdUnit = GetRewardData(adUnit);
        if (rewardDataByAdUnit == null)
        {
            Debug.LogError("AdModel.SetRewardEarned - no reward data for adUnit " + adUnit);
            return;
        }
        this.adData.requested.Remove(rewardDataByAdUnit);
        this.adData.dirty = true;
        if (adUnit == AdUnits.Rewarded)
            this.adData.earnedRewardDayOfYear.Add(TimeUtils.GetDayOfYear());
        if (this.adData.earnedRewardDayOfYear.Count > 50)
        {
            this.adData.earnedRewardDayOfYear.RemoveAt(0);
        }
        this.OnRewardEarned?.Invoke(rewardDataByAdUnit);
    }

    public int CountRewardedAdsToday()
    {
        var today = TimeUtils.GetDayOfYear();
        return this.adData.earnedRewardDayOfYear.Sum(dayOfYear => dayOfYear == today ? 1 : 0);
    }

    public void SetRewardFailed(string adUnit)
    {
        if (string.IsNullOrEmpty(adUnit))
        {
            Debug.LogError("AdModel.SetRewardFailed - adUnit is null or empty");
            return;
        }
        //Debug.Log("AdModel.SetRewardFailed " + adUnit);
        var rewardDataByAdUnit = GetRewardData(adUnit);
        if (rewardDataByAdUnit == null)
        {
            Debug.LogError("AdModel.SetRewardFailed - no reward data for adUnit " + adUnit);
            return;
        }
        this.adData.requested.Remove(rewardDataByAdUnit);
        this.OnRewardFailed?.Invoke(rewardDataByAdUnit);
    }

    public bool IsAdReady(string adUnit)
    {
        return this.adUnitsReady.Contains(adUnit);
    }

    public bool IsAdReady(RewardName rewardName)
    {
        return IsAdReady(AdUnits.GetAdUnitForReward(rewardName));
    }

    public AdRewardData GetRewardData(string adUnit)
    {
        return this.adData.requested.FindLast(x => x.adUnit == adUnit);
    }
}
