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
