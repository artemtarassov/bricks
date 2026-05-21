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
}

[Serializable]
public class SlotElementDataList
{
    public int columnIndex;
    public List<SlotElementData> list = new List<SlotElementData>();
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
}

public enum BrickState
{
    Transparent = 1,
    Flying = 3,
    Full = 4,
    Colored = 5,
}
