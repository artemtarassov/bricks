using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public int lastDailyRewardTimestamp = 0;
    public int installTimestamp = 0;
    public int unlockedBuildings = 0;
    public int coins = 0;
    public int attempts = 0;
    public int additionalEmitterUnlockTimeoutTimestamp = 0; //-1 means unlocked permanently, 0 means locked, >0 means unlocked temporarily until the timestamp

    public CityElementDataContainer currentElement = null;
    public string currentGroupName = null;

    public List<SettingsKey> enabledSettings;

    [System.NonSerialized]
    public bool isDirty;
}