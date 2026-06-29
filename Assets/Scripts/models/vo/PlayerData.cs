using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


[Serializable]
public class BuildingProgressData
{
    [SerializeField]
    private BuildingName buildingName = BuildingName.Undefined;

    public BuildingName BuildingName => buildingName;

    [SerializeField]
    private int completedBuildingCounter = 0;

    [SerializeField]
    private int completedElementsCounter = 0;

    public int CompletedElementsCounter => completedElementsCounter;

    public int CompletedBuildingCounter => completedBuildingCounter;

    [SerializeField]
    private BuildingState state = BuildingState.Locked;

    public BuildingState State => state;

    public int attempts = -1;


    [SerializeField]
    public int currentElementIndex = 0;

    [SerializeField]
    private CityElementDataContainer currentElement = null;

    public CityElementDataContainer GetCurrentElement()
    {
        return currentElement != null && !string.IsNullOrEmpty(currentElement.dataKey) ? currentElement : null;
    }

    public void SetState(BuildingState newState)
    {
        state = newState;
    }

    public void RemoveCurrentElement()
    {
        currentElement = null;
    }

    public void SetCurrentElement(CityElementDataContainer element)
    {
        Debug.Log($"BuildingProgressData: SetCurrentElement: setting current element to {element.dataKey} for building {buildingName}");
        Assert.IsNotNull(element, "BuildingProgressData: SetCurrentElement: element should not be null");
        Assert.IsFalse(string.IsNullOrEmpty(element.dataKey), "BuildingProgressData: SetCurrentElement: element dataKey should not be null or empty");
        this.currentElement = element;
    }

    public void ResetElementsCounter()
    {
        this.completedElementsCounter = 0;
    }

    public void IncCompletedElementsCounter()
    {
        this.completedElementsCounter++;
    }

    public void IncCompletedBuildingCounter()
    {
        this.completedBuildingCounter++;
    }

    public void ResetCompletedBuildingCounter()
    {
        this.completedBuildingCounter = 0;
    }

    public BuildingProgressData(BuildingName buildingName, BuildingState state)
    {
        Assert.IsFalse(buildingName == BuildingName.Undefined, "BuildingProgressData: buildingName should not be undefined");
        this.buildingName = buildingName;
        this.state = state;
    }

}

[Serializable]
public class PlayerData
{
    public string appVersion;
    public int secondsPlaying = 0;
    public int lastDailyRewardTimestamp = 0;
    public int installTimestamp = 0;
    public int coins = 0;
    public int currentBuildingAttempts
    {
        set
        {
            GetCurrentBuildingProgress().attempts = value;
            isDirty = true;
        }
        get
        {
            return GetCurrentBuildingProgress().attempts;
        }
    }
    public int additionalEmitterUnlockTimeoutTimestamp = 0; //-1 means unlocked permanently, 0 means locked, >0 means unlocked temporarily until the timestamp
    public int difficultyIndex = -1;

    public List<BuildingProgressData> Progress => progress;

    [SerializeField]
    private List<BuildingProgressData> progress = new List<BuildingProgressData>();

    public List<BuildingName> allBuildingNames => progress.ConvertAll(p => p.BuildingName);

    [SerializeField]
    private BuildingName currentBuildingName = BuildingName.Undefined;

    public BuildingName CurrentBuildingName => currentBuildingName;

    public void SetCurrentBuilding(BuildingName buildingName, BuildingState newState)
    {
        Assert.IsFalse(buildingName == BuildingName.Undefined, "PlayerData: SetCurrentBuilding: buildingName should not be undefined");
        this.currentBuildingName = buildingName;
        this.GetCurrentBuildingProgress().SetState(newState);
        isDirty = true;
    }

    public void RemoveCurrentBuilding()
    {
        this.currentBuildingName = BuildingName.Undefined;
        isDirty = true;
    }


    public BuildingProgressData GetBuildingProgressByName(BuildingName buildingName)
    {
        return progress.Find(p => p.BuildingName == buildingName);
    }


    public BuildingProgressData GetCurrentBuildingProgress()
    {
        if (currentBuildingName == BuildingName.Undefined)
        {
            return null;
        }
        return progress.Find(g => g.BuildingName == currentBuildingName);
    }

    public void EnableSetting(SettingsKey key)
    {
        if (!enabledSettings.Contains(key))
        {
            enabledSettings.Add(key);
            isDirty = true;
        }
    }
    public void DisableSetting(SettingsKey key)
    {
        if (enabledSettings.Contains(key))
        {
            enabledSettings.Remove(key);
            isDirty = true;
        }
    }

    [SerializeField]
    public List<SettingsKey> enabledSettings;

    [System.NonSerialized]
    public bool isDirty;
}