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
    }

    public List<string> GetPlayableGroupNames()
    {
        return playerData.progress.FindAll(g => g.state != GroupState.Locked).ConvertAll(g => g.groupName);
    }

    public void SetCurrentGroup(GroupState newState)
    {
        this.SetCurrentGroup(playerData.currentGroupName, newState);
    }

    public void SetCurrentGroup(string groupName, GroupState newState)
    {
        var progress = playerData.progress.Find(p => p.groupName == groupName);
        if (progress == null)
        {
            Debug.LogError($"PlayerModel: SetCurrentGroup: no progress found for group {groupName}");
            return;
        }
        if (progress.state == GroupState.Playing && newState == GroupState.Completed)
        {
            progress.completedCounter += 1;
        }
        progress.state = newState;
        playerData.currentGroupName = groupName;
        playerData.isDirty = true;
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
            coins = 10000,
            unlockedBuildings = 0,
            attempts = 1,
            installTimestamp = TimeUtils.GetUnixTimestamp(),
            isDirty = true,
            progress = new List<GroupProgressData>()
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

        if (playerData.currentElement != null)
        {
            this.playerData.currentElement.brickDataList.ForEach((e) => e.ResetEmittingStates());
            this.playerData.currentElement.columns.ForEach((s) => s.ResetEmittingStates());
        }
    }


}