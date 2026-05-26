using System;
using System.Collections.Generic;
using System.Linq;


[Serializable]
public class BrickData
{
    public ColorIndex color;
    public int coloredAmount => this.states.Count(s => s == BrickState.Colored);
    public int transparentAmount => this.states.Count(s => s == BrickState.Transparent);
    public int emittingAmount => this.states.Count(s => s == BrickState.Emitting);
    public int fullAmount => this.states.Count(s => s == BrickState.Full);
    public int max => this.states.Count;

    public bool AllTransparent => this.states.All(s => s == BrickState.Transparent);
    public bool AllFull => this.states.All(s => s == BrickState.Full);

    public void SetAllColored()
    {
        for (int i = 0; i < this.states.Count; i++)
        {
            this.states[i] = BrickState.Colored;
        }
    }

    public void SetAllTransparent()
    {
        for (int i = 0; i < this.states.Count; i++)
        {
            this.states[i] = BrickState.Transparent;
        }
    }

    public List<BrickState> states = new List<BrickState>();

    public int GetBrickIndex(BrickState state)
    {
        return this.states.FindIndex(s => s == state);
    }

    public BrickState GetBrickState(int index)
    {
        if (index < 0 || index >= this.states.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Index should be between 0 and {this.states.Count - 1}, but was {index}");
        }
        return this.states[index];
    }

    public void SetBrickState(int index, BrickState state)
    {
        if (index < 0 || index >= this.states.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Index should be between 0 and {this.states.Count - 1}, but was {index}");
        }
        this.states[index] = state;
    }

    public BrickData(int max, ColorIndex color)
    {
        this.color = color;
        for (int i = 0; i < max; i++)
        {
            this.states.Add(BrickState.Transparent);
        }
    }

    public BrickData Clone()
    {
        var clone = new BrickData(this.max, this.color);
        for (int i = 0; i < this.states.Count; i++)
        {
            clone.states[i] = this.states[i];
        }
        return clone;
    }
}