using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class AddExtrasCmd
{
    private CityElementDataContainer dataContainer;
    private int coins => PlayerModel.Instance.playerData.coins;
    private int attempts => PlayerModel.Instance.playerData.attempts;
    public void Run(CityElementDataContainer currentElementData)
    {
        Debug.Log($"AddExtrasCmd: adding extras to element {currentElementData.dataKey}");
        Assert.IsNotNull(currentElementData, "AddExtrasCmd Run: data container is null");
        Assert.IsTrue(currentElementData.columns.Count > 0, "AddExtrasCmd Run: data container should have at least 1 column");
        var totalBricks = currentElementData.columns.Sum(c => c.list.Count(e => e.IsBrick));
        Assert.IsTrue(totalBricks > 0, "AddExtrasCmd Run: data container should have at least 1 brick to add extras");
        this.dataContainer = currentElementData;

        var extrasApplied = currentElementData.columns.Any(c => c.list.Any(e => !e.IsBrick));
        if (extrasApplied)
        {
            return;
        }
        ApplyDifficulty();
        // AddExplosion();
    }

    private bool ShouldAddAd()
    {
        var installTimestamp = PlayerModel.Instance.playerData.installTimestamp;
        var secPassed = TimeUtils.GetUnixTimestamp() - installTimestamp;
        if (secPassed < 60 * 5)
        {
            //don't add ad for the first 5 minutes after install
            return false;
        }
        var hasGoldenTicket = IAPModel.Instance.DidPurchaseComplete(IAPProductName.GoldenTicket) || IAPModel.Instance.HasTempGoldenTicket();
        if (hasGoldenTicket)
        {
            //don't add ad if player has golden ticket
            return false;
        }
        var didLoadAd = AdModel.Instance.IsAdReady(RewardName.MID_SESSION_INTERSTITIAL) || AdModel.Instance.IsAdReady(RewardName.MID_SESSION_REWARDED);
        return didLoadAd;
    }


    private void ApplyDifficulty()
    {
        var difficulties = new int[] { 0, 1, 2, 3, 4, 5, 6, 2 };
        var pd = PlayerModel.Instance.playerData;

        pd.difficultyIndex++;
        if (pd.difficultyIndex >= difficulties.Length)
        {
            pd.difficultyIndex = 0;
        }
        pd.isDirty = true;

        var difficultyToApply = difficulties[pd.difficultyIndex];

        if (difficultyToApply == 0)
        {
            AddCoinsIfAbsent();
            return;
        }

        if (difficultyToApply == 1)
        {
            AddAd();
            AddBicksMultiplier();
            return;
        }
        if (difficultyToApply == 2)
        {
            AddCoinsIfAbsent();
            SetHiddenBricks(1);
            return;
        }
        if (difficultyToApply == 3)
        {
            AddAd();
            AddDeath(1);
            AddBicksMultiplier();
            return;
        }
        if (difficultyToApply == 4)
        {
            AddCoinsIfAbsent();
            SetHiddenBricks(1);
            AddDeath(1);
            return;
        }
        if (difficultyToApply == 5)
        {
            AddAd();
            SetHiddenBricks(2);
            AddDeath(attempts > 0 ? 2 : 1);
            AddBicksMultiplier();
            return;
        }
        if (difficultyToApply == 6)
        {
            AddCoinsIfAbsent();
            SetHiddenBricks(2);
            AddDeath(2);
            return;
        }

    }


    private void SetHiddenBricks(int amount = 1)
    {
        var countExisting = dataContainer.columns.Sum(c => c.list.Count(e => e.type == SlotElementType.HiddenBricks));
        if (countExisting >= amount)
        {
            return;
        }
        var columns = dataContainer.columns.ToList();
        RandHelper.Shuffle(columns);

        for (int i = 0; i < columns.Count && amount > 0; i++)
        {
            var randColumn = columns[i];
            var allBrickElements = randColumn.list.Where(e => e.IsBrick).ToList();
            if (allBrickElements.Count < 5)
            {
                continue;
            }
            var randIndex = RandHelper.GetRandIndex(allBrickElements, 2, RandHelper.RandPos.SecondHalf);
            var randBrick = allBrickElements[randIndex];
            randBrick.type = SlotElementType.HiddenBricks;
            amount--;
        }
    }

    private bool HasSlotElementType(SlotElementType type)
    {
        return dataContainer.columns.Any(c => c.list.Any(e => e.type == type));
    }


    private static NonRepeatingShuffleBag<int> deadCounter = new NonRepeatingShuffleBag<int>(new List<int> { 1, 1, 1, 2, 2, 3, 4 });

    private void AddDeath(int amount = 1)
    {
        var columns = dataContainer.columns.FindAll((c) => c.list.Count > 5);
        if (columns.Count == 0)
        {
            return;
        }
        RandHelper.Shuffle(columns);

        for (var i = 0; i < columns.Count && amount > 0; i++)
        {
            var column = columns[i];
            var randIndex = RandHelper.GetRandIndex(column.list, 1, RandHelper.RandPos.Random);
            column.list.Insert(randIndex, new SlotElementData(SlotElementType.EmitterDeathWaiting)
            {
                deadCounter = deadCounter.GetNext()
            });
            amount--;
        }
    }


    private void AddExplosion()
    {
        var hasExplosion = HasSlotElementType(SlotElementType.FinalExplosion);
        if (hasExplosion)
        {
            Debug.LogError("AddExtrasCmd: explosion already present, skipping adding explosion");
            return;
        }
        var randColumn = RandHelper.GetRandomElement(dataContainer.columns);
        randColumn.list.Add(new SlotElementData(SlotElementType.FinalExplosion));
        Debug.Log("AddExtrasCmd: explosion added to column " + randColumn.columnIndex);
    }

    private void AddBicksMultiplier()
    {
        var hasMult = HasSlotElementType(SlotElementType.AddMoreBricks);
        if (hasMult)
        {
            return;
        }
        var columnsWithBricks = dataContainer.columns.Where(c => c.list.All(e => e.IsBrick)).ToList();
        if (columnsWithBricks.Count == 0)
        {
            return;
        }
        var randColumn = RandHelper.GetRandomElement(columnsWithBricks);
        var prevLength = randColumn.list.Count;
        if (prevLength <= 3)
        {
            return;
        }
        var randIndex =  RandHelper.GetRandIndex(randColumn.list, 1, RandHelper.RandPos.SecondThird);
        //var randIndex = 0;
        randColumn.list.Insert(randIndex, new SlotElementData(SlotElementType.AddMoreBricks));
        Assert.AreEqual(prevLength + 1, randColumn.list.Count, "UnlockCityElementCmd AddBicksMultiplier: failed to add additional bricks multiplier to column");
    }

    private void AddCoinsIfAbsent()
    {
        var hasCoins = HasSlotElementType(SlotElementType.Coins);
        if (hasCoins)
        {
            return;
        }
        var randColumn = RandHelper.GetRandomElement(dataContainer.columns);
        var prevLength = randColumn.list.Count;
        if (prevLength <= 3)
        {
            return;
        }
        if (coins < 1000 || Random.value > 0.9f)
        {
            var randIndex = RandHelper.GetRandIndex(randColumn.list, 1, RandHelper.RandPos.LastThird);
            randColumn.list.Insert(randIndex, new SlotElementData(SlotElementType.Coins));
        }
    }

    private void AddAd()
    {
        var hasAd = HasSlotElementType(SlotElementType.Ad);
        if (hasAd)
        {
            return;
        }
        if (!ShouldAddAd())
        {
            return;
        }
        var totalBricks = dataContainer.columns.Sum(c => c.list.Count(e => e.IsBrick));
        if (totalBricks <= 10)
        {
            //small element
            return;
        }
        var randColumn = RandHelper.GetRandomElement(dataContainer.columns);
        var prevLength = randColumn.list.Count;
        if (prevLength < 5)
        {
            return;
        }
        var randIndex = RandHelper.GetRandIndex(randColumn.list, 1, RandHelper.RandPos.SecondHalf);
        //var randIndex = 0;
        randColumn.list.Insert(randIndex, new SlotElementData(SlotElementType.Ad));
        Assert.AreEqual(prevLength + 1, randColumn.list.Count, "UnlockCityElementCmd AddAd: failed to add ad to column");
    }

}