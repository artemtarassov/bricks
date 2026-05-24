using UnityEngine;

public class PlayerModel
{
    public static PlayerModel Instance;

    public PlayerData playerData { get; private set; }

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
            coins = 1000,
            unlockedBuildings = 0,
            attempts = 5,
            installTimestamp = TimeUtils.GetUnixTimestamp(),
            isDirty = true
        };
    }

    public void AddCoins(int amount)
    {
        this.playerData.coins += amount;
        this.playerData.isDirty = true;
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