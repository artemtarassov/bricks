using System.Collections.Generic;

public class PlayerData
{
    public int installTimestamp = 0;
    public int unlockedBuildings = 0;
    public int coins = 0;
    public int attempts = 0;

    public int emitterUnlockTimestamp = 0;
    public int emitterIndex = 0;

    public CityElementDataContainer last = null;

    [System.NonSerialized]
    public bool isDirty;
}