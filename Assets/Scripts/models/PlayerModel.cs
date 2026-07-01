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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var data = JsonUtility.ToJson(this.playerData, true);
#else
        var data = JsonUtility.ToJson(this.playerData, false);
#endif
        FilePrefs.SetString(savekey, data);
        this.playerData.isDirty = false;
    }


    public void SetCurrentBuilding(BuildingState newState)
    {
        this.SetCurrentBuilding(playerData.CurrentBuildingName, newState);
    }

    public void SetCurrentBuilding(BuildingName buildingName, BuildingState newState)
    {
        //Debug.Log($"PlayerModel: setting current building to {buildingName} with state {newState}");
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
            playerData.EnableSetting(key);
            OnPlayerDataChanged?.Invoke();
        }
        else if (!enable && isEnabled)
        {
            playerData.DisableSetting(key);
            OnPlayerDataChanged?.Invoke();
        }
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
            installTimestamp = TimeUtils.GetUnixTimestamp(),
            isDirty = true,
        };
        this.playerData.DisableSetting(SettingsKey.NightMode);
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

    public bool FillAttempts(BuildingName buildingName, int amount, int max)
    {
        var progress = this.playerData.GetBuildingProgressByName(buildingName);
        progress.attempts = Math.Min(progress.attempts + amount, max);
        this.playerData.isDirty = true;
        OnPlayerDataChanged?.Invoke();
        return true;
    }


    public bool UseAttempt(BuildingName buildingName)
    {
        var progress = this.playerData.GetBuildingProgressByName(buildingName);
        if (progress.attempts <= 0)
        {
            return false;
        }
        progress.attempts -= 1;
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

    }

    public void AddTimeoutSeconds(int n)
    {
        var progress = playerData.GetCurrentBuildingProgress();
        var element = progress.GetCurrentElement();
        element.AddTimeoutSeconds(n);
        playerData.isDirty = true;
    }


}
