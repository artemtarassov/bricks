using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class CityElementColors
{
    private static RoundRobinList<int> roundRobinAmount = new RoundRobinList<int>(new List<int>()
        { 5,5,5,10,10,10,15}
);
    private static RoundRobinList<ColorIndex> roundRobinColorIndex = new RoundRobinList<ColorIndex>(new List<ColorIndex>()
        { ColorIndex.C0, ColorIndex.C1, ColorIndex.C2, ColorIndex.C3, ColorIndex.C4, ColorIndex.C5 }
    );

    public readonly List<BrickData> predefinedBricks = new List<BrickData>();
    private int colorIndexPointer = 0;
    public List<BrickData> NextColorIndexList(int maxDifferentColors)
    {
        Assert.IsTrue(maxDifferentColors > 0, "invalid maxDifferentColors amount ");
        var result = new List<BrickData>();
        for (var i = 0; i < maxDifferentColors && this.colorIndexPointer < this.predefinedBricks.Count; i++)
        {
            var colorData = this.predefinedBricks[this.colorIndexPointer];
            result.Add(colorData);
            this.colorIndexPointer++;
        }
        return result;
    }

    public CityElementColors(int bricksToAdd)
    {
        Assert.IsTrue(bricksToAdd > 0, "invalid bricks amount ");
        var cnt = bricksToAdd;
        while (bricksToAdd > 0)
        {
            var amount = roundRobinAmount.GetNext();
            var clr = roundRobinColorIndex.GetNext();
            this.predefinedBricks.Add(new BrickData() { color = clr, amount = amount });
            bricksToAdd -= amount;
        }
        if (bricksToAdd < 0)
        {
            var last = this.predefinedBricks.Last();
            last.amount += bricksToAdd;
        }
        var predefinedSum = this.predefinedBricks.Sum(b => b.amount);
        Assert.AreEqual(cnt, predefinedSum, $"Sum of predefined bricks should match the number of bricks to add. " +
            $"Bricks to add: {bricksToAdd}, sum of predefined bricks: {predefinedSum}");
    }
}