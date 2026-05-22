using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SlotModel
{
    public static readonly int MaxColumns = 3;
    public static SlotModel Instance;
    public List<SlotElementDataList> Columns { get; private set; }

    public BrickData[] Emitters { get; private set; }

    public Action OnColumnsChanged;
    public Action<int> OnEmitterChanged;
    public Action<BrickData, int> OnBrickMovedFromColumnToEmitter;

    public SlotModel()
    {
        this.Emitters = new BrickData[2];
        this.Columns = new List<SlotElementDataList>();
        for (var i = 0; i < MaxColumns; i++)
        {
            this.Columns.Add(new SlotElementDataList() { columnIndex = i });
        }
    }

    public bool HasBricksInEmitters()
    {
        return this.Emitters.Any(e => e != null && e.amount > 0);
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
        //        Debug.Log($"Adding brick to slot: column {column}, color {bd.color}, amount {bd.amount}");
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
        int index = Array.FindIndex(this.Emitters, e => e == bd);
        if (bd.amount < 1)
        {
            this.Emitters[index] = null;
        }
        OnEmitterChanged?.Invoke(index);
    }

    public bool HasEmitterSpace()
    {
        return Array.FindIndex(this.Emitters, e => e == null) != -1;
    }

    public int GetEmptyEmitterIndex()
    {
        return Array.FindIndex(this.Emitters, e => e == null);
    }

    private int AddEmitter(BrickData brickData)
    {
        Assert.IsTrue(brickData.amount > 0);
        Assert.IsTrue(brickData.color != ColorIndex.Undefined);
        Assert.IsTrue(Array.FindIndex(this.Emitters, e => e == brickData) == -1);

        var emptyIndex = GetEmptyEmitterIndex();
        this.Emitters[emptyIndex] = brickData;
        return emptyIndex;
    }

    public bool MoveFromColumnToEmitter(SlotElementData sed)
    {
        foreach (var c in this.Columns)
        {
            var element = c.list.Find(e => e == sed);
            if (element != null)
            {
                c.list.Remove(element);
                var emitterIndex = AddEmitter(sed.brickData);
                this.OnBrickMovedFromColumnToEmitter?.Invoke(sed.brickData, emitterIndex);
                return true;
            }
        }
        return false;
    }

}