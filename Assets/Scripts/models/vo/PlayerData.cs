using System;
using System.Collections.Generic;

[Serializable]

public enum GroupState
{
    Locked = 1,
    Unlocked = 2,
    Playing = 3,
    Completed = 4
}

[Serializable]

public class GroupProgressData
{
    public string groupName;
    public int completedGroupCounter = 0;
    public int completedElementsCounter = 0;
    public GroupState state;
    public CityElementDataContainer currentElement = null;

}

[Serializable]
public class PlayerData
{
    public int secondsPlaying = 0;
    public int lastDailyRewardTimestamp = 0;
    public int installTimestamp = 0;
    public int coins = 0;
    public int attempts = 0;
    public int additionalEmitterUnlockTimeoutTimestamp = 0; //-1 means unlocked permanently, 0 means locked, >0 means unlocked temporarily until the timestamp
    public int difficultyIndex = 0;

    public List<GroupProgressData> progress = null;

    public List<string> allGroupNames => progress.ConvertAll(p => p.groupName);
    public string currentGroupName { get; private set; }
    public void SetCurrentGroup(string groupName)
    {
        currentGroupName = groupName;
        isDirty = true;
    }

    public GroupProgressData GetCurrentGroupProgress()
    {
        return progress.Find(g => g.groupName == currentGroupName);
    }

    public CityElementDataContainer currentElement
    {
        get
        {
            return GetCurrentGroupProgress().currentElement;
        }
        set
        {
            isDirty = true;
            GetCurrentGroupProgress().currentElement = value;
        }
    }

    public List<SettingsKey> enabledSettings;

    [System.NonSerialized]
    public bool isDirty;
}