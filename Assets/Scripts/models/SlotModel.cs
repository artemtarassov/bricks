using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class SlotModel
{
    public static readonly int MaxColumns = 3;
    public static readonly int MaxEmitters = 4;
    public static readonly int AdditionalEmitterIndex = MaxEmitters - 1;

    public static SlotModel Instance;
    public List<SlotColumnData> Columns { get; private set; }

    public List<EmitterSpace> Emitters { get; private set; }

    public Action OnColumnsChanged;
    public Action<EmitterSpace> OnEmitterChanged;
    public Action<BrickData, int> OnBrickMovedFromColumnToEmitter;
    public Action<SlotElementData> OnRemovedFromColumn;


    public SlotModel()
    {
        this.Emitters = new List<EmitterSpace>();
        for (var i = 0; i < MaxEmitters; i++)
        {
            this.Emitters.Add(new EmitterSpace() { index = i, isUnlocked = i < AdditionalEmitterIndex });
        }
        this.Columns = new List<SlotColumnData>();
        for (var i = 0; i < MaxColumns; i++)
        {
            this.Columns.Add(new SlotColumnData() { columnIndex = i });
        }
    }


    public void UpdateEmitters(EmitterSpace es)
    {
        if (es.brickData != null && es.brickData.AllFull)
        {
            es.brickData = null;
        }
        OnEmitterChanged?.Invoke(es);
    }

    public void LockAdditionalEmitter()
    {
        var emitter = this.Emitters.Find((e) => e.index == AdditionalEmitterIndex);
        emitter.isUnlocked = false;
        emitter.brickData = null;
        this.OnEmitterChanged?.Invoke(emitter);
    }

    public int GetLockedEmitterIndex()
    {
        var lockedEmitter = this.Emitters.Find(e => !e.isUnlocked);
        return lockedEmitter != null ? lockedEmitter.index : -1;
    }

    public void UnlockAdditionalEmitter()
    {
        var e = this.Emitters.Find(e => e.index == AdditionalEmitterIndex);
        e.isUnlocked = true;
        this.OnEmitterChanged?.Invoke(e);
    }

    public bool HasBricksInEmitters()
    {
        return this.Emitters.Any(e => e.HasColoredBricks);
    }

    public void Clear()
    {
        for (var i = 0; i < this.Emitters.Count; i++)
        {
            this.Emitters[i].brickData = null;
        }
        this.Columns = new List<SlotColumnData>();
        this.OnEmitterChanged?.Invoke(null);
        this.OnColumnsChanged?.Invoke();
    }

    public void Fill(List<SlotColumnData> columns)
    {
        var hasExplosive = columns.Any(c => c.list.Any(e => e.type == SlotElementType.FinalExplosion));
        Assert.IsTrue(hasExplosive, "SlotModel Fill: columns should contain explosive elements");

        for (var i = 0; i < this.Emitters.Count; i++)
        {
            this.Emitters[i].brickData = null;
        }
        Assert.IsTrue(columns.Count > 0, "City element should have at least 1 column of slot data");
        foreach (var column in columns)
        {
            foreach (var element in column.list)
            {
                if (element.type == SlotElementType.Bricks && element.brickData.inEmitter && element.brickData.coloredAmount > 0)
                {
                    var emptyIndex = GetEmptyEmitterIndex();
                    if (emptyIndex >= 0)
                        this.Emitters[emptyIndex].brickData = element.brickData;
                }
            }
        }
        this.Columns = columns;
        this.OnEmitterChanged?.Invoke(null);
        this.OnColumnsChanged?.Invoke();
    }

    public void RemoveAds()
    {
        var slotsToRemove = this.Columns.SelectMany(c => c.list).Where(e => e.type == SlotElementType.Ad).ToList();
        foreach (var slot in slotsToRemove)
        {
            Replace(slot, SlotElementType.Undefined);
        }
    }

    public bool HasEmitterSpace()
    {
        return this.Emitters.FindAll((e) => e.IsEmpty).Count > 0;
    }

    public int CountEmptyEmitters()
    {
        return this.Emitters.FindAll((e) => e.IsEmpty).Count;
    }

    public int GetEmptyEmitterIndex()
    {
        var e = this.Emitters.Find((e) => e.IsEmpty);
        return e != null ? e.index : -1;
    }


    public SlotElementData GetNextSlotElementDataInColumn(int columnIndex, int rowIndex = 0)
    {
        var slotColumn = Columns.Find(c => c.columnIndex == columnIndex);
        var list = slotColumn.list.FindAll(e => e.type != SlotElementType.Undefined && e.IsInEmitter() == false);
        if (list.Count <= rowIndex)
        {
            return null;
        }
        var elementData = list[rowIndex];
        return elementData;
    }

    private int AddToUnlockedEmitter(BrickData brickData)
    {
        Assert.IsTrue(brickData.coloredAmount > 0);
        Assert.IsTrue(brickData.color != ColorIndex.Undefined);
        var inEmitter = this.Emitters.Exists(e => e.brickData == brickData);
        Assert.IsFalse(inEmitter, "Brick data is already in emitter");
        var emptyIndex = GetEmptyEmitterIndex();
        Assert.IsTrue(emptyIndex >= 0, "No empty emitter found");
        brickData.inEmitter = true;
        this.Emitters[emptyIndex].brickData = brickData;
        this.OnEmitterChanged?.Invoke(this.Emitters[emptyIndex]);
        return emptyIndex;
    }

    public void MoveFromColumnToEmitter(BrickData brickData)
    {
        Assert.IsNotNull(brickData, "Brick data must not be null");
        Assert.IsTrue(brickData.coloredAmount > 0, "Only bricks with colored amount can be moved to emitter");

        Assert.IsTrue(brickData.color != ColorIndex.Undefined, "Brick color must be defined");
        Assert.IsTrue(this.Columns.Any(c => c.list.Any(e => e.brickData == brickData)), "Brick data not found in any column");
        Assert.IsFalse(brickData.inEmitter, "Brick data is already in emitter");

        var emitterIndex = AddToUnlockedEmitter(brickData);
        Assert.IsTrue(emitterIndex >= 0, "No empty emitter found");
        this.OnBrickMovedFromColumnToEmitter?.Invoke(brickData, emitterIndex);
    }

    public void Replace(SlotElementData sed, SlotElementType t)
    {
        var column = this.Columns.Find(c => c.list.Any(e => e == sed));
        Assert.IsNotNull(column, "Column not found for SlotElementData");
        var element = column.list.FindIndex(e => e == sed);
        Assert.IsTrue(element >= 0, "SlotElementData not found in column");
        column.list[element] = new SlotElementData(t);
        this.OnRemovedFromColumn?.Invoke(sed);
    }

    public void Replace(BrickData sed, SlotElementType t)
    {
        var column = this.Columns.Find(c => c.list.Any(e => e.brickData == sed));
        Assert.IsNotNull(column, "Column not found for BrickData");
        var e = column.list.Find(e => e.brickData == sed);
        this.Replace(e, t);
    }

}