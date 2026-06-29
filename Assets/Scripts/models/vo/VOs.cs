using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum GameOverReason
{
    Undefined = 0,
    OutOfSpace = 1,
    OutOfTime = 2
}

public class ToastMsg
{
    public string text;
    public BuildingName challenge = BuildingName.Undefined;
}

[Serializable]

public enum FinishElementType
{
    Undefined = 0,
    SlideDown = 100,
    Explosion = 200
}


[Serializable]

public enum BuildingState
{
    Locked = 1,
    Unlocked = 2,
    Playing = 3,
    Completed = 4,
    Premium = 5,
}

[Serializable]
public enum BuildingName
{
    Undefined = 0,
    Preset_House_05 = 100,
    Ruins1_House = 200,
    Tower_House = 300,
    Preset_Bath_House_01 = 400,

    Challenge_House_Cat01 = 100100,
    Challenge_House_Rabbit01 = 100200,
    Challenge_House_Bear01 = 100300,
    Challenge_House_Shark01 = 100400,
    Challenge_House_Frog01 = 100500,
    Challenge_House_Octopus01 = 100600,
    Challenge_House_Sailbot01 = 100700,
    Challenge_House_HotChocolate01 = 100800,
    Challenge_House_Crystals01 = 100900,
    Challenge_House_Cake01 = 101000

}

public class BuildingNameUtil
{
    public static readonly List<BuildingName> allBuildingNamesRegular = new List<BuildingName>() {
        BuildingName.Preset_House_05,
        BuildingName.Ruins1_House,
        BuildingName.Tower_House,
        BuildingName.Preset_Bath_House_01
    };
    public static readonly List<BuildingName> allBuildingNamesChallenges = new List<BuildingName>() {
        BuildingName.Challenge_House_Cat01,
        BuildingName.Challenge_House_Rabbit01,
        BuildingName.Challenge_House_Bear01,
        BuildingName.Challenge_House_Shark01,
        BuildingName.Challenge_House_Frog01,
        BuildingName.Challenge_House_Octopus01,
        BuildingName.Challenge_House_Sailbot01,
        BuildingName.Challenge_House_HotChocolate01,
        BuildingName.Challenge_House_Crystals01,
        BuildingName.Challenge_House_Cake01
    };
    private static readonly List<BuildingName> allBuildingNamesAll = allBuildingNamesRegular.Concat(allBuildingNamesChallenges).ToList();

    public static List<BuildingName> GetAllBuildingNames(bool includeChallenges)
    {
        return includeChallenges ? allBuildingNamesAll : allBuildingNamesRegular;
    }
    public static BuildingName GetBuildingNameByString(string name)
    {
        var result = GetAllBuildingNames(true).Find(b => b.ToString() == name);
        return result;
    }

    public static bool IsPremiumBuilding(BuildingName buildingName)
    {
        return buildingName == BuildingName.Preset_Bath_House_01;
    }

    public static bool IsChallengeBuilding(BuildingName buildingName)
    {
        return allBuildingNamesChallenges.Contains(buildingName);
    }
}

[Serializable]
public enum SettingsKey
{
    Undefined = 0,
    Sounds = 1,
    Music = 2,
    Vibrations = 3,
    NightMode = 4,
}


[Serializable]
public enum IAPProductName
{
    Undefined = 0,
    GoldenTicket = 1,
    GoldenTicketTemp = 2,
    AdditionalSpace = 3,
    PremiumBuilding1 = 4,
}



[Serializable]
public enum SlotElementType
{
    Undefined = 0,
    Bricks = 1,
    HiddenBricks = 2,
    AddMoreBricks = 3,
    Coins = 4,
    Ad = 5,
    FinalExplosion = 6,
    EmitterDeathWaiting = 7,
    EmitterDeathActive = 8,
    UnlockChallenge = 9,
    AddSecondsInChallenge = 10//only for challenges
}

[Serializable]
public class SlotElementData
{
    public SlotElementType type = SlotElementType.Undefined;

    public bool IsBrick => type == SlotElementType.Bricks || type == SlotElementType.HiddenBricks;

    [SerializeField]
    private BrickData brickData = null;

    public int deadCounter = 0;

    public int secondsToAdd = 0; //only for challenges

    public BuildingName challenge = BuildingName.Undefined;

    public BrickData BrickData
    {
        get
        {
            if (this.IsBrick)
            {
                return this.brickData;
            }
            return null;
        }
        set
        {
            if (this.IsBrick)
            {
                this.brickData = value;
            }
            else
            {
                if (value != null)
                    throw new InvalidOperationException($"SlotElementData: cannot set BrickData for type {this.type}");
                this.brickData = null;
            }
        }
    }

    public bool IsVisible()
    {
        if (this.type == SlotElementType.Undefined || this.type == SlotElementType.EmitterDeathActive)
        {
            return false;
        }
        if (this.IsBrick)
        {
            return !this.BrickData.inEmitter;
        }
        return true;
    }

    public void ResetEmittingStates()
    {
        if (BrickData != null)
        {
            BrickData.ResetEmittingStates();
        }
    }

    public SlotElementData()
    {
        this.type = SlotElementType.Undefined;
        this.BrickData = null;
    }
    public SlotElementData(BrickData brickData)
    {
        this.type = SlotElementType.Bricks;
        this.BrickData = brickData;
    }
    public SlotElementData(SlotElementType type)
    {
        this.type = type;
        this.BrickData = null;
    }
    public SlotElementData Clone()
    {
        return new SlotElementData()
        {
            type = this.type,
            brickData = (this.BrickData != null) ? this.BrickData.Clone() : null,
            deadCounter = this.deadCounter
        };
    }
}

[Serializable]
public class SlotColumnData
{
    public int columnIndex;
    public List<SlotElementData> list = new List<SlotElementData>();

    public SlotColumnData Clone()
    {
        var clone = new SlotColumnData();
        clone.columnIndex = this.columnIndex;
        foreach (var item in this.list)
        {
            clone.list.Add(item.Clone());
        }
        return clone;
    }
    public void ResetEmittingStates()
    {
        this.list.ForEach((e) => e.ResetEmittingStates());
    }

    public int GetNextBrickIndex(int atIndex)
    {
        while (atIndex < list.Count)
        {
            var e = list[atIndex];
            if (e.type == SlotElementType.Bricks)
            {
                return atIndex;
            }
            atIndex++;
        }
        return -1;
    }

    public bool IsEmpty()
    {
        foreach (var e in this.list)
        {
            switch (e.type)
            {
                case SlotElementType.Bricks:
                case SlotElementType.HiddenBricks:
                    if (e.BrickData.coloredAmount > 0)
                    {
                        return false;
                    }
                    break;
                case SlotElementType.AddMoreBricks:
                case SlotElementType.Coins:
                    return false;
            }
        }
        return true;
    }
}

[Serializable]
public enum ColorIndex
{
    Undefined = -1,
    C0 = 0,
    C1 = 1,
    C2 = 2,
    C3 = 3,
    C4 = 4,
    C5 = 5,
    C6 = 6,
}


[Serializable]
public enum BrickState
{
    Undefined = 0,
    Transparent = 1,
    SemiTransparent = 2,
    Emitting = 3,
    Full = 4,
    Colored = 5,
}


public class EmitterSpace
{
    public BrickData brickData = null;
    public int index;
    public bool isUnlocked = false;
    public bool isDead => deadCounter > 0;
    public bool HasColoredBricks => isUnlocked && brickData != null && brickData.coloredAmount > 0;
    public bool IsEmpty => isDead == false && isUnlocked && brickData == null;
    public int deadCounter = 0;

    public void Reset()
    {
        this.brickData = null;
        this.deadCounter = 0;
    }
}


[Serializable]
public class BuildingDataContainer
{
    public List<BuildingData> buildings = new List<BuildingData>();
}


[Serializable]
public class BuildingData
{
    [SerializeField]
    private BuildingName buildingName = BuildingName.Undefined;

    public BuildingName BuildingName => buildingName;

    public List<CityElementDataContainer> cityElementDataList;

    public BuildingData(BuildingName n)
    {
        this.buildingName = n;
        this.cityElementDataList = new List<CityElementDataContainer>();
    }

    public CityElementDataContainer GetElementDataContainerByIndex(int index)
    {
        if (index < 0 || index >= cityElementDataList.Count)
        {
            return null;
        }
        return cityElementDataList[index];
    }

    public BuildingData Clone()
    {
        var clone = new BuildingData(this.buildingName);
        foreach (var item in this.cityElementDataList)
        {
            clone.cityElementDataList.Add(item.Clone());
        }
        return clone;
    }
}
