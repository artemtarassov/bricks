using System.Collections.Generic;

public class NonRepeatingShuffleBag<T>
{
    private List<T> shuffledItems;
    private int nextIndex = 0;

    public int Count => this.shuffledItems.Count;

    public NonRepeatingShuffleBag(List<T> list)
    {
        this.nextIndex = 0;
        this.shuffledItems = new List<T>(list);
        RandHelper.Shuffle(this.shuffledItems);
    }

    public T GetNext()
    {
        var item = this.shuffledItems[this.nextIndex];
        this.nextIndex++;
        if (this.nextIndex >= this.shuffledItems.Count)
        {
            this.nextIndex = 0;
            this.ReshuffleAvoidingImmediateRepeat(item);
        }
        return item;
    }

    private void ReshuffleAvoidingImmediateRepeat(T previousItem)
    {
        if (this.shuffledItems.Count <= 1)
        {
            return;
        }

        RandHelper.Shuffle(this.shuffledItems);
        if (!EqualityComparer<T>.Default.Equals(this.shuffledItems[0], previousItem))
        {
            return;
        }

        var eligibleIndexes = new List<int>();
        for (var i = 1; i < this.shuffledItems.Count; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(this.shuffledItems[i], previousItem))
            {
                eligibleIndexes.Add(i);
            }
        }

        if (eligibleIndexes.Count == 0)
        {
            return;
        }

        var swapIndex = eligibleIndexes[RandHelper.GetRandomInt(0, eligibleIndexes.Count)];
        var firstItem = this.shuffledItems[0];
        this.shuffledItems[0] = this.shuffledItems[swapIndex];
        this.shuffledItems[swapIndex] = firstItem;
    }
}
