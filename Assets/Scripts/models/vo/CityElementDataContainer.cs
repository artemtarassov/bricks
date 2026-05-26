
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
    public List<SlotElementDataList> slotElementDataList;

    public int ElementCountColoredBricks(ColorIndex colorIndex)
    {
        return this.brickDataList.Sum(bd => bd.color == colorIndex ? bd.coloredAmount : 0);
    }

    public int ElementCountTransparentBricks()
    {
        return this.brickDataList.Sum(bd => bd.transparentAmount);

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

    public bool SlotsHaveBricks()
    {
        Assert.IsNotNull(this.slotElementDataList, "slotElementDataList should not be null");
        Assert.IsTrue(this.slotElementDataList.Count > 0, "slotElementDataList should not be empty");
        foreach (var slotElementDataList in this.slotElementDataList)
        {
            Assert.IsNotNull(slotElementDataList.list, "slotElementDataList.list should not be null");
            Assert.IsTrue(slotElementDataList.list.Count > 0, "slotElementDataList.list should not be empty");
            if (slotElementDataList.list.Exists(s => s.brickData != null && s.brickData.coloredAmount > 0))
            {
                return true;
            }
        }
        return false;
    }


    public void EnableDifferentColors(int amountOfColors)
    {
        var transparentBricks = this.brickDataList.FindAll(bd => bd.AllTransparent);
        for (var i = 0; i < amountOfColors && i < transparentBricks.Count; i++)
        {
            transparentBricks[i].SetAllColored();
        }
    }

    public void Reset()
    {
        foreach (var brickData in this.brickDataList)
        {
            brickData.SetAllTransparent();
        }
        foreach (var slotData in this.slotElementDataList)
        {
            foreach (var col in slotData.list)
            {
                if (col.brickData != null)
                {
                    col.brickData.SetAllColored();
                }
            }
        }
    }

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
        clone.slotElementDataList = new List<SlotElementDataList>();
        foreach (var item in this.slotElementDataList)
        {
            clone.slotElementDataList.Add(item.Clone());
        }
        return clone;
    }
}