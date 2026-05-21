using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SlotModel
{
    public static readonly SlotModel Instance = new SlotModel();
    public List<SlotElementDataList> Columns { get; private set; }

    public BrickData[] Emitters { get; private set; }

    public Action<BrickData> OnSlotsChanged;
    public Action<BrickData> OnEmitterChanged;

    public SlotModel()
    {
        this.Emitters = new BrickData[2];
        this.Columns = new List<SlotElementDataList>()
        {
            new SlotElementDataList(),
            new SlotElementDataList(),
            new SlotElementDataList(),
            new SlotElementDataList(),
        };
    }

    public int CountBricks()
    {
        var count = 0;
        foreach (var column in this.Columns)
        {
            count += column.list.Sum(b => b.type == SlotElementType.Bricks ? b.brickData.amount : 0);
        }
        return count;
    }

    public void ClearAll()
    {
        foreach (var column in this.Columns)
        {
            column.list.Clear();
        }
        for (var i = 0; i < this.Emitters.Length; i++)
        {
            this.Emitters[i] = null;
        }
    }

    public void AddBricksToColumn(int column, BrickData bd)
    {
        Assert.IsTrue(column >= 0 && column < this.Columns.Count, "Column index out of range");
        Assert.IsTrue(bd.amount > 0, "Brick data amount must be greater than 0");
        Assert.IsTrue(bd.color != ColorIndex.Undefined, "Brick data color index must be defined");
        Debug.Log($"Adding brick to slot: column {column}, color {bd.color}, amount {bd.amount}");
        this.Columns[column].list.Add(new SlotElementData(bd));
    }

    public void AddMoreBricksToColumn(int column)
    {
        Assert.IsTrue(column >= 0 && column < this.Columns.Count, "Column index out of range");
        this.Columns[column].list.Add(new SlotElementData(SlotElementType.AddMoreBricks));
    }

    public void DecrementEmitter(BrickData bd)
    {
        Assert.IsTrue(bd.amount > 0, "Emitter amount must be greater than 0");
        Assert.IsTrue(Array.Exists(this.Emitters, e => e == bd), "Emitter must be in emitters list");
        bd.amount--;
        if (bd.amount < 1)
        {
            int index = Array.FindIndex(this.Emitters, e => e == bd);
            this.Emitters[index] = null;
        }
        OnEmitterChanged?.Invoke(bd);
    }

    public bool HasEmitterSpace()
    {
        return Array.FindIndex(this.Emitters, e => e == null) != -1;
    }

    public void AddEmitter(BrickData brickData)
    {
        Assert.IsTrue(brickData.amount > 0);
        Assert.IsTrue(brickData.color != ColorIndex.Undefined);
        Assert.IsTrue(Array.FindIndex(this.Emitters, e => e == brickData) == -1);

        var emptyIndex = Array.FindIndex(this.Emitters, e => e == null);
        this.Emitters[emptyIndex] = brickData;
        this.OnEmitterChanged?.Invoke(brickData);
    }

    public bool RemoveFromColumn(SlotElementData sed)
    {
        foreach (var c in this.Columns)
        {
            var element = c.list.Find(e => e == sed);
            if (element != null)
            {
                c.list.Remove(element);
                this.OnSlotsChanged?.Invoke(sed.brickData);
                return true;
            }
        }
        return false;
    }

}