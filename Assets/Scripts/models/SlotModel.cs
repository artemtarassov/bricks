using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SlotModel
{
    public static readonly int MaxColumns = 3;
    public static readonly int MaxEmitters = 3;
    public static SlotModel Instance;
    public List<SlotElementDataList> Columns { get; private set; }

    public List<EmitterSpace> Emitters { get; private set; }

    public Action OnColumnsChanged;
    public Action<EmitterSpace> OnEmitterChanged;
    public Action<BrickData, int> OnBrickMovedFromColumnToEmitter;

    public SlotModel()
    {
        this.Emitters = new List<EmitterSpace>();
        for (var i = 0; i < MaxEmitters; i++)
        {
            this.Emitters.Add(new EmitterSpace() { index = i, isUnlocked = i < 2 });
        }
        this.Columns = new List<SlotElementDataList>();
        for (var i = 0; i < MaxColumns; i++)
        {
            this.Columns.Add(new SlotElementDataList() { columnIndex = i });
        }
    }

    public void LockEmitter(int emitterIndex)
    {
        var emitter = this.Emitters.Find((e) => e.index == emitterIndex);
        emitter.isUnlocked = false;
        emitter.brickData = null;
        this.OnEmitterChanged?.Invoke(emitter);
    }

    public int UnlockNextEmitter()
    {
        var lockedEmitter = this.Emitters.Find(e => !e.isUnlocked);
        Assert.IsNotNull(lockedEmitter, "No locked emitter found");
        lockedEmitter.isUnlocked = true;
        this.OnEmitterChanged?.Invoke(lockedEmitter);
        return lockedEmitter.index;
    }

    public bool HasBricksInEmitters()
    {
        return this.Emitters.Any(e => e.HasBricks);
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

    private void ClearAll()
    {
        foreach (var column in this.Columns)
        {
            column.list.Clear();
        }
        for (var i = 0; i < this.Emitters.Count; i++)
        {
            this.Emitters[i].brickData = null;
        }
    }

    public void Fill(List<SlotElementDataList> slotElementDataList)
    {
        ClearAll();
        Assert.IsTrue(slotElementDataList.Count > 1, "City element should have at least 2 columns of slot data");
        foreach (var sedl in slotElementDataList)
        {
            var columnIndex = sedl.columnIndex;
            foreach (var sed in sedl.list)
            {
                if (sed.type == SlotElementType.Bricks)
                {
                    this.AddBricksToColumn(columnIndex, sed.brickData);
                }
                else if (sed.type == SlotElementType.AddMoreBricks)
                {
                    this.AddMoreBricksToColumn(columnIndex);
                }
            }
        }
        this.OnColumnsChanged?.Invoke();

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

    public void DecrementEmitter(int emitterIndex)
    {
        var emitter = this.Emitters[emitterIndex];
        Assert.IsNotNull(emitter);
        Assert.IsTrue(emitter.HasBricks);
        var bd = emitter.brickData;
        bd.amount--;
        if (bd.amount < 1)
        {
            emitter.brickData = null;
        }
        OnEmitterChanged?.Invoke(emitter);
    }

    public bool HasEmitterSpace()
    {
        return this.Emitters.Any((e) => e.IsEmpty);
    }

    public int CountEmptyEmitters()
    {
        return this.Emitters.Count((e) => e.IsEmpty);
    }

    public int GetEmptyEmitterIndex()
    {
        var e = this.Emitters.Find((e) => e.IsEmpty);
        return e != null ? e.index : -1;
    }

    private int AddToUnlockedEmitter(BrickData brickData)
    {
        Assert.IsTrue(brickData.amount > 0);
        Assert.IsTrue(brickData.color != ColorIndex.Undefined);
        var emptyIndex = GetEmptyEmitterIndex();
        Assert.IsTrue(emptyIndex >= 0, "No empty emitter found");
        this.Emitters[emptyIndex].brickData = brickData;
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
                var emitterIndex = AddToUnlockedEmitter(sed.brickData);
                Assert.IsTrue(emitterIndex >= 0, "No empty emitter found");
                this.OnBrickMovedFromColumnToEmitter?.Invoke(sed.brickData, emitterIndex);
                return true;
            }
        }
        return false;
    }

}