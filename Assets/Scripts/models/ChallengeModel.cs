using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ChallengeData
{
    public BuildingName buildingName = BuildingName.Undefined;
    public bool isLocked = true;
}

public class ChallengeDataContainer
{
    public List<ChallengeData> list = new List<ChallengeData>();
    public bool isDirty = false;
    public int lastUnlockedTimestamp = 0;
    public int currentChallengeStartTimestamp = 0;
    public int currentChallengeEndTimestamp = 0;
}

public class ChallengeModel
{
    public static ChallengeModel Instance;

    private ChallengeDataContainer dataContainer;

    public Action OnStarted;
    public Action OnStopped;

    private static readonly string savekey = "challenges";


    public void Save()
    {
        if (this.dataContainer == null || !this.dataContainer.isDirty)
        {
            return;
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var data = JsonUtility.ToJson(this.dataContainer, true);
#else
        var data = JsonUtility.ToJson(this.dataContainer, false);
#endif
        FilePrefs.SetString(savekey, data);
        this.dataContainer.isDirty = false;
    }

    public List<ChallengeData> GetAllChallenges()
    {
        return this.dataContainer.list;
    }

    public void UnlockChallenge(BuildingName buildingName)
    {
        Assert.IsTrue(BuildingNameUtil.IsChallengeBuilding(buildingName), $"ChallengeModel UnlockChallenge: building {buildingName} is not a challenge building");
        GetChallengeData(buildingName).isLocked = false;
        this.dataContainer.isDirty = true;
        this.dataContainer.lastUnlockedTimestamp = TimeUtils.GetUnixTimestamp();
    }

    public void StartChallenge(int bricks)
    {
        this.dataContainer.currentChallengeStartTimestamp = TimeUtils.GetUnixTimestamp();
        this.dataContainer.currentChallengeEndTimestamp = this.dataContainer.currentChallengeStartTimestamp + bricks * 10;
        this.dataContainer.isDirty = true;
        this.OnStarted?.Invoke();
    }

    public void StopChallenge()
    {
        if (this.dataContainer.currentChallengeEndTimestamp == 0)
        {
            return;
        }
        this.dataContainer.currentChallengeStartTimestamp = 0;
        this.dataContainer.currentChallengeEndTimestamp = 0;
        this.dataContainer.isDirty = true;
        this.OnStopped?.Invoke();
    }

    public int GetSecondsLeft()
    {
        if (this.dataContainer.currentChallengeEndTimestamp == 0)
        {
            return -1;
        }
        var now = TimeUtils.GetUnixTimestamp();
        var secondsLeft = this.dataContainer.currentChallengeEndTimestamp - now;
        return Mathf.Max(0, secondsLeft);
    }

    public int GetLastUnlockedTimestamp()
    {
        return this.dataContainer.lastUnlockedTimestamp;
    }

    public bool HasUnlockedChallenges()
    {
        return this.dataContainer.list.Exists((a) => a.isLocked == false);
    }

    public BuildingName GetLastUnlockedChallenge()
    {
        var data = this.dataContainer.list.FindLast((a) => a.isLocked == false);
        if (data == null)
        {
            return BuildingName.Undefined;
        }
        return data.buildingName;
    }

    public BuildingName GetNextLockedChallenge()
    {
        var data = this.dataContainer.list.Find((a) => a.isLocked == true);
        if (data == null)
        {
            return BuildingName.Undefined;
        }
        return data.buildingName;
    }

    private void CreateNewData()
    {
        this.dataContainer = new ChallengeDataContainer();
    }

    public void Load()
    {
        var data = FilePrefs.GetString(savekey, "");
        if (string.IsNullOrEmpty(data))
        {
            this.CreateNewData();
        }
        else
        {
            try
            {
                this.dataContainer = JsonUtility.FromJson<ChallengeDataContainer>(data);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ChallengeModel Load: failed to parse player data json, error: {e}");
                this.CreateNewData();
            }
        }

        var list = BuildingNameUtil.allBuildingNamesChallenges;
        foreach (var b in list)
        {
            var hasChallenge = GetChallengeData(b) != null;
            if (hasChallenge == false)
            {
                this.dataContainer.list.Add(new ChallengeData()
                {
                    buildingName = b,
                    isLocked = true
                });
            }
        }

#if UNITY_EDITOR
        foreach (var b in this.dataContainer.list)
        {
            b.isLocked = false;
        }
#endif
    }

    private ChallengeData GetChallengeData(BuildingName buildingName)
    {
        Assert.IsTrue(BuildingNameUtil.IsChallengeBuilding(buildingName), $"ChallengeModel GetChallengeData: building {buildingName} is not a challenge building");
        return this.dataContainer.list.Find((a) => a.buildingName == buildingName);
    }

}