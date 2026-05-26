using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class CityElementColors
{
    private static NonRepeatingShuffleBag<int> shuffledAmounts => BalancingModel.shuffledAmounts;
    private static NonRepeatingShuffleBag<ColorIndex> shuffledColorIndexes => BalancingModel.shuffledColorIndexes;

    public readonly List<BrickData> predefinedBricks = new List<BrickData>();

    public CityElementColors(int bricksToAdd)
    {
        Assert.IsTrue(bricksToAdd > 0, "invalid bricks amount ");
        var cnt = bricksToAdd;
        while (bricksToAdd > 0)
        {
            var amount = shuffledAmounts.GetNext();
            var clr = shuffledColorIndexes.GetNext();
            this.predefinedBricks.Add(new BrickData(amount, clr));
            bricksToAdd -= amount;
        }
        if (bricksToAdd < 0)
        {
            var last = this.predefinedBricks.Last();
            this.predefinedBricks.Remove(last);
            last = new BrickData(last.max + bricksToAdd, last.color);
            this.predefinedBricks.Add(last);
        }
        var predefinedSum = this.predefinedBricks.Sum(b => b.transparentAmount);
        Assert.AreEqual(cnt, predefinedSum, $"Sum of predefined bricks should match the number of bricks to add. " +
            $"Bricks to add: {bricksToAdd}, sum of predefined bricks: {predefinedSum}");
    }
}
