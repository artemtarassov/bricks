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
            this.Emitters.Add(new EmitterSpace() { index = i, isUnlocked = i < AdditionalEmitterIndex });
        }
        this.Columns = new List<SlotElementDataList>();
        for (var i = 0; i < MaxColumns; i++)
        {
            this.Columns.Add(new SlotElementDataList() { columnIndex = i });
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

    public void Fill(List<SlotElementDataList> slotElementDataList)
    {
        for (var i = 0; i < this.Emitters.Count; i++)
        {
            this.Emitters[i].brickData = null;
        }
        Assert.IsTrue(slotElementDataList.Count > 1, "City element should have at least 2 columns of slot data");
        this.Columns = slotElementDataList;
        this.OnEmitterChanged?.Invoke(null);
        this.OnColumnsChanged?.Invoke();
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

    private int AddToUnlockedEmitter(BrickData brickData)
    {
        Assert.IsTrue(brickData.coloredAmount > 0);
        Assert.IsTrue(brickData.color != ColorIndex.Undefined);
        var emptyIndex = GetEmptyEmitterIndex();
        Assert.IsTrue(emptyIndex >= 0, "No empty emitter found");
        this.Emitters[emptyIndex].brickData = brickData;
        this.OnEmitterChanged?.Invoke(this.Emitters[emptyIndex]);
        return emptyIndex;
    }

    public bool MoveFromColumnToEmitter(BrickData brickData)
    {
        Assert.IsNotNull(brickData, "Brick data must not be null");
        Assert.IsTrue(brickData.coloredAmount > 0, "Only bricks with colored amount can be moved to emitter");
    
        Assert.IsTrue(brickData.color != ColorIndex.Undefined, "Brick color must be defined");
        Assert.IsTrue(this.Columns.Any(c => c.list.Any(e => e.brickData == brickData)), "Brick data not found in any column");

        foreach (var c in this.Columns)
        {
            var emitterIndex = AddToUnlockedEmitter(brickData);
            Assert.IsTrue(emitterIndex >= 0, "No empty emitter found");
            this.OnBrickMovedFromColumnToEmitter?.Invoke(brickData, emitterIndex); 
            return true;
        }
        return false;
    }

}