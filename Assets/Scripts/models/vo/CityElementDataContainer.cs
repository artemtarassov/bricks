
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class CityElementDataContainer
{
    public string dataKey;
    public List<BrickData> brickDataList;
    public List<SlotColumnData> columns;

    public int ElementCountColoredBricks(ColorIndex colorIndex)
    {
        return this.brickDataList.Sum(bd => bd.color == colorIndex ? bd.coloredAmount : 0);
    }

    public int ElementCountColoredBricks()
    {
        return this.brickDataList.Sum(bd => bd.coloredAmount);
    }

    public int ElementCountEmittingBricks()
    {
        return this.brickDataList.Sum(bd => bd.emittingAmount);
    }

    public bool ElementCompleted()
    {
        return this.brickDataList.All(bd => bd.fullAmount == bd.max);
    }

    /*public BrickData GetNextColoredBrick(ColorIndex colorIndex)
    {
        return this.brickDataList.Find(bd => bd.state == BrickState.Colored && bd.color == colorIndex && bd.amount > 0);
    }*/

    public bool AllSlotsEmpty()
    {
        Assert.IsNotNull(this.columns, "slotElementDataList should not be null");
        Assert.IsTrue(this.columns.Count > 0, "slotElementDataList should not be empty");
        return this.columns.All(c => c.IsEmpty());
    }


    public void EnableDifferentColors(int amountOfColors)
    {
        var transparentBricks = this.brickDataList.FindAll(bd => bd.AllTransparent);
        var last = Math.Min(amountOfColors, transparentBricks.Count);
        for (var i = 0; i < last; i++)
        {
            transparentBricks[i].SetAll(BrickState.Colored);
            var n = i + amountOfColors;
            if (n < transparentBricks.Count)
                transparentBricks[n].SetAll(BrickState.SemiTransparent);
        }
    }

    /*public void Reset()
    {
        foreach (var brickData in this.brickDataList)
        {
            brickData.SetAllTransparent();
        }
        foreach (var slotData in this.columns)
        {
            foreach (var col in slotData.list)
            {
                if (col.brickData != null)
                {
                    col.brickData.SetAllColored();
                }
            }
        }
    }*/

    public HashSet<ColorIndex> GetBrickColors()
    {
        var result = new HashSet<ColorIndex>();
        foreach (var bd in this.brickDataList)
        {
            if (bd.coloredAmount > 0 || bd.emittingAmount > 0)
            {
                result.Add(bd.color);
            }
        }
        return result;
    }


    public CityElementDataContainer Clone()
    {
        var clone = new CityElementDataContainer();
        clone.dataKey = this.dataKey;
        clone.brickDataList = new List<BrickData>();
        foreach (var item in this.brickDataList)
        {
            clone.brickDataList.Add(item.Clone());
        }
        clone.columns = new List<SlotColumnData>();
        foreach (var item in this.columns)
        {
            clone.columns.Add(item.Clone());
        }
        return clone;
    }
}