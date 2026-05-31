using System;
using System.Collections.Generic;
using System.Linq;




[Serializable]
public enum SlotElementType
{
    Undefined = 0,
    Bricks = 1,
    HiddenBricks = 2,
    AddMoreBricks = 3,
    Coins = 4
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
        this.type = SlotElementType.Undefined;
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



    public bool IsEmpty()
    {
        foreach (var e in this.list)
        {
            switch (e.type)
            {
                case SlotElementType.Bricks:
                case SlotElementType.HiddenBricks:
                    if (e.brickData.coloredAmount > 0)
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

    public GroupDataList Clone()
    {
        var clone = new GroupDataList(this.groupName);
        foreach (var item in this.cityElementDataList)
        {
            clone.cityElementDataList.Add(item.Clone());
        }
        return clone;
    }
}
