using System;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerModel
{
    public static PlayerModel Instance;

    public PlayerData playerData { get; private set; }

    public Action OnPlayerDataChanged;

    public void Save()
    {
        if (this.playerData == null || !this.playerData.isDirty)
        {
            return;
        }
        var data = JsonUtility.ToJson(this.playerData);
        FilePrefs.SetString("player_data", data);
        this.playerData.isDirty = false;
    }

    private void CreateNewPlayerData()
    {
        this.playerData = new PlayerData()
        {
            coins = 10000,
            unlockedBuildings = 0,
            attempts = 1,
            installTimestamp = TimeUtils.GetUnixTimestamp(),
            isDirty = true
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
        Debug.Log($"PlayerModel: adding {amount} coins");
        this.playerData.coins += amount;
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
        var data = FilePrefs.GetString("player_data", "");
        if (string.IsNullOrEmpty(data))
        {
            this.CreateNewPlayerData();
            return;
        }
        try
        {
            this.playerData = JsonUtility.FromJson<PlayerData>(data);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerModel Load: failed to parse player data json, error: {e}");
            this.CreateNewPlayerData();
        }


    }
}