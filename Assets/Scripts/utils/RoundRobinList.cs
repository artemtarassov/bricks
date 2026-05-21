using System.Collections.Generic;

public class RoundRobinList<T>
{
    private List<T> shuffledItems;
    private int nextIndex = 0;

    public int Count => this.shuffledItems.Count;

    public RoundRobinList(List<T> list)
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
            RandHelper.Shuffle(this.shuffledItems);
        }
        return item;
    }

}
