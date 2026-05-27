using System;
using System.Collections.Generic;
using System.Linq;




[Serializable]
public enum SlotElementType
{
    Empty,
    Bricks,
    HiddenBricks,
    AddMoreBricks,
    Coins
}

[Serializable]
public class SlotElementData
{
    public SlotElementType type;
    public BrickData brickData = null;

    public bool IsInEmitterSpace()
    {
        if (this.brickData == null)
        {
            return false;
        }
        return this.brickData.emittingAmount > 0;
    }
    public void ResetEmittingStates()
    {
        if (brickData != null)
        {
            brickData.ResetEmittingStates();
        }
    }

    public SlotElementData()
    {
        this.type = SlotElementType.Empty;
        this.brickData = null;
    }
    public SlotElementData(BrickData brickData)
    {
        this.type = SlotElementType.Bricks;
        this.brickData = brickData;
    }
    public SlotElementData(SlotElementType type)
    {
        this.type = type;
        this.brickData = null;
    }
    public SlotElementData Clone()
    {
        return new SlotElementData()
        {
            type = this.type,
            brickData = this.brickData != null ? this.brickData.Clone() : null
        };
    }
}

[Serializable]
public class SlotElementDataList
{
    public int columnIndex;
    public List<SlotElementData> list = new List<SlotElementData>();

    public SlotElementDataList Clone()
    {
        var clone = new SlotElementDataList();
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
    Emitting = 3,
    Full = 4,
    Colored = 5,
}

[Serializable]
public class EmitterSpace
{
    public BrickData brickData = null;
    public int index;
    public bool isUnlocked = false;
    public bool HasColoredBricks => isUnlocked && brickData != null && brickData.coloredAmount > 0;
    public bool IsEmpty => isUnlocked && brickData == null;
}


[Serializable]
public class GroupDataListContainer
{
    public List<GroupDataList> groups = new List<GroupDataList>();
}


[Serializable]
public class GroupDataList
{
    public string groupName;
    public List<CityElementDataContainer> cityElementDataList;
    public GroupDataList(string n)
    {
        this.groupName = n;
        this.cityElementDataList = new List<CityElementDataContainer>();
    }

    public void Reset()
    {
        foreach (var cityElementDataContainer in this.cityElementDataList)
        {
            cityElementDataContainer.Reset();
        }
    }

    public GroupDataList Clone()
    {
        var clone = new GroupDataList(this.groupName);
        foreach (var item in this.cityElementDataList)
        {
            clone.cityElementDataList.Add(item.Clone());
        }
        return clone;
    }

    public bool HasBricks()
    {
        foreach (var cityElementDataContainer in this.cityElementDataList)
        {
            if (cityElementDataContainer.SlotsHaveBricks())
            {
                return true;
            }
        }
        return false;
    }
}
