using System;
using System.Collections.Generic;


[Serializable]
public class BrickData
{
    public ColorIndex color = ColorIndex.Undefined;
    public int amount = 0;

    public BrickData Clone()
    {
        return new BrickData() { color = this.color, amount = this.amount };
    }
}

[Serializable]
public enum SlotElementType
{
    Empty,
    Bricks,
    HiddenBricks,
    AddMoreBricks,
}

[Serializable]
public class SlotElementData
{
    public SlotElementType type;
    public BrickData brickData = null;

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

public enum BrickState
{
    Transparent = 1,
    Flying = 3,
    Full = 4,
    Colored = 5,
}

[Serializable]
public class EmitterSpace
{
    public BrickData brickData = null;
    public int index;
    public bool isUnlocked = false;
    public bool HasBricks => isUnlocked && brickData != null && brickData.amount > 0;
    public bool IsEmpty => isUnlocked && brickData == null;
}


[Serializable]
public class DataContainerList
{
    public List<CityElementDataContainer> list;
}

[Serializable]
public class CityElementDataContainer
{
    public string key;
    public List<BrickData> brickDataList;
    public List<SlotElementDataList> slotElementDataList;

    public CityElementDataContainer Clone()
    {
        var clone = new CityElementDataContainer();
        clone.key = this.key;
        clone.brickDataList = new List<BrickData>();
        foreach (var item in this.brickDataList)
        {
            clone.brickDataList.Add(item.Clone());
        }
        clone.slotElementDataList = new List<SlotElementDataList>();
        foreach (var item in this.slotElementDataList)
        {
            clone.slotElementDataList.Add(item.Clone());
        }
        return clone;
    }
}