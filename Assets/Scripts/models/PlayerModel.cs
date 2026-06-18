using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerModel
{
    public static PlayerModel Instance;

    public PlayerData playerData { get; private set; }

    public Action OnPlayerDataChanged;

    private readonly string savekey = "playerdata";

    public void Save()
    {
        if (this.playerData == null || !this.playerData.isDirty)
        {
            return;
        }
        var data = JsonUtility.ToJson(this.playerData, true);
        FilePrefs.SetString(savekey, data);
        this.playerData.isDirty = false;
        FilePrefs.Save();
        Debug.Log("PlayerModel saved " + data.Length + " bytes. content " + data);
    }


    public void SetCurrentBuilding(BuildingState newState)
    {
        this.SetCurrentBuilding(playerData.CurrentBuildingName, newState);
    }

    public void SetCurrentBuilding(BuildingName buildingName, BuildingState newState)
    {
        Debug.Log($"PlayerModel: setting current building to {buildingName} with state {newState}");
        playerData.SetCurrentBuilding(buildingName, newState);
        OnPlayerDataChanged?.Invoke();
    }
    public void RemoveCurrentBuilding()
    {
        playerData.RemoveCurrentBuilding();
        OnPlayerDataChanged?.Invoke();
    }

    public void EnableSetting(SettingsKey key, bool enable)
    {
        var isEnabled = IsSettingEnabled(key);
        if (enable && !isEnabled)
        {
            playerData.enabledSettings.Add(key);
            playerData.isDirty = true;
            OnPlayerDataChanged?.Invoke();
        }
        else if (!enable && isEnabled)
        {
            playerData.enabledSettings.Remove(key);
            playerData.isDirty = true;
            OnPlayerDataChanged?.Invoke();
        }
    }

    public List<BuildingName> GetUnlockedBuildings()
    {
        var allBuildingNames = BuildingNameUtil.GetAllBuildingNames();
        var unlockedBuildings = new List<BuildingName>();
        foreach (var buildingName in allBuildingNames)
        {
            var progress = playerData.GetBuildingProgressByName(buildingName);
            if (progress != null && progress.State != BuildingState.Locked)
            {
                unlockedBuildings.Add(buildingName);
            }
        }
        return unlockedBuildings;
    }

    public bool IsSettingEnabled(SettingsKey key)
    {
        return playerData.enabledSettings.Contains(key);
    }

    private void CreateNewPlayerData()
    {
        var allSettingsKeys = Enum.GetValues(typeof(SettingsKey)).Cast<SettingsKey>().ToList().FindAll(s => s != SettingsKey.Undefined);
        this.playerData = new PlayerData()
        {
            enabledSettings = allSettingsKeys,
            coins = 0,
            attempts = 5,
            installTimestamp = TimeUtils.GetUnixTimestamp(),
            isDirty = true,
        };
    }


    public void LockAdditionalEmitter()
    {
        if (playerData.additionalEmitterUnlockTimeoutTimestamp == -1)
        {
            return; //permanently unlocked, do not lock
        }
        playerData.additionalEmitterUnlockTimeoutTimestamp = 0;
        playerData.isDirty = true;
        OnPlayerDataChanged?.Invoke();
    }

    public void UnlockAdditionalEmitter(int additionalEmitterUnlockTimeoutTimestamp = -1)
    {
        if (playerData.additionalEmitterUnlockTimeoutTimestamp == -1)
        {
            return; //permanently unlocked, do not lock
        }
        playerData.additionalEmitterUnlockTimeoutTimestamp = additionalEmitterUnlockTimeoutTimestamp;
        playerData.isDirty = true;
        OnPlayerDataChanged?.Invoke();
    }

    public bool CanAfford(int cost)
    {
        return this.playerData.coins >= cost;
    }

    public void AddCoins(int amount)
    {
        //Debug.Log($"PlayerModel: adding {amount} coins");
        this.playerData.coins += amount;
        this.playerData.isDirty = true;
        OnPlayerDataChanged?.Invoke();
    }

    public void AddDailyRewardCoins(int amount)
    {
        //Debug.Log($"PlayerModel: adding {amount} daily reward coins");
        this.playerData.coins += amount;
        this.playerData.lastDailyRewardTimestamp = TimeUtils.GetUnixTimestamp();
        this.playerData.isDirty = true;
        OnPlayerDataChanged?.Invoke();
    }

    public bool FillAttempts(int amount, int max)
    {
        if (this.playerData.attempts >= max)
        {
            return false;
        }
        this.playerData.attempts = Math.Min(this.playerData.attempts + amount, max);
        this.playerData.isDirty = true;
        OnPlayerDataChanged?.Invoke();
        return true;
    }


    public bool UseAttempt()
    {
        if (this.playerData.attempts <= 0)
        {
            return false;
        }
        Debug.Log("PlayerModel: using attempt, attempts left before use: " + this.playerData.attempts);
        this.playerData.attempts -= 1;
        this.playerData.isDirty = true;
        OnPlayerDataChanged?.Invoke();
        return true;
    }

    public void Load()
    {
        var data = FilePrefs.GetString(savekey, "");
        if (string.IsNullOrEmpty(data))
        {
            this.CreateNewPlayerData();
            return;
        }
        try
        {
            Debug.Log("PlayerModel loaded " + data.Length + " bytes");
            this.playerData = JsonUtility.FromJson<PlayerData>(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerModel Load: failed to parse player data json, error: {e}");
            this.CreateNewPlayerData();
        }

        foreach (var p in playerData.Progress)
        {
            if (p.State == BuildingState.Playing)
            {
                p.SetState(BuildingState.Unlocked); //reset in-progress building to unlocked, so player can replay it
            }
        }

        var progress = playerData.GetCurrentBuildingProgress();
        if (progress != null && progress.GetCurrentElement() != null)
        {
            var element = progress.GetCurrentElement();
            element.brickDataList.ForEach((e) => e.ResetEmittingStates());
            element.columns.ForEach((s) => s.ResetEmittingStates());
        }


        
    }


}