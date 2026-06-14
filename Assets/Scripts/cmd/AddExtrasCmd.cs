using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class AddExtrasCmd
{
    private CityElementDataContainer currentElementData;
    public void Run(CityElementDataContainer currentElementData)
    {
        Debug.Log($"AddExtrasCmd: adding extras to element {currentElementData.dataKey}");
        Assert.IsNotNull(currentElementData, "AddExtrasCmd Run: data container is null");
        Assert.IsTrue(currentElementData.columns.Count > 0, "AddExtrasCmd Run: data container should have at least 1 column");
        var totalBricks = currentElementData.columns.Sum(c => c.list.Count(e => e.type == SlotElementType.Bricks));
        Assert.IsTrue(totalBricks > 0, "AddExtrasCmd Run: data container should have at least 1 brick to add extras");
        this.currentElementData = currentElementData;

        var extrasApplied = currentElementData.columns.Any(c => c.list.Any(e => e.type == SlotElementType.FinalExplosion));
        if (extrasApplied)
        {
            return;
        }
        ApplyDifficulty();
        AddExplosion();
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
        var difficulties = new int[] { 0, 0, 1, 2, 3, 4, 2 };
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
            AddCoinsIfAbsent(currentElementData);
            return;
        }

        if (difficultyToApply == 1)
        {
            AddCoinsIfAbsent(currentElementData);
            AddBicksMultiplier(currentElementData);
            return;
        }
        if (difficultyToApply == 2)
        {
            if (ShouldAddAd())
                AddAd(currentElementData);
            AddCoinsIfAbsent(currentElementData);
            AddBicksMultiplier(currentElementData);
            SetHiddenBricks(currentElementData, 1);
            return;
        }
        if (difficultyToApply == 3)
        {
            AddBicksMultiplier(currentElementData);
            SetHiddenBricks(currentElementData, 2);
            return;
        }
        if (difficultyToApply == 4)
        {
            if (ShouldAddAd())
                AddAd(currentElementData);
            AddCoinsIfAbsent(currentElementData);
            SetHiddenBricks(currentElementData, 2);
            return;
        }

    }


    private void SetHiddenBricks(CityElementDataContainer dataContainer, int amount = 1)
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
            var allBrickElements = randColumn.list.Where(e => e.type == SlotElementType.Bricks).ToList();
            if (allBrickElements.Count < 5)
            {
                continue;
            }
            var randIndex = Random.Range(1, allBrickElements.Count - 2);
            var randBrick = allBrickElements[randIndex];
            randBrick.type = SlotElementType.HiddenBricks;
            amount--;
        }
    }


    private void AddExplosion()
    {
        var hasExplosion = currentElementData.columns.Any(s => s.list.Any(e => e.type == SlotElementType.FinalExplosion));
        if (hasExplosion)
        {
            Debug.LogError("AddExtrasCmd: explosion already present, skipping adding explosion");
            return;
        }
        var randColumn = RandHelper.GetRandomElement(currentElementData.columns);
        randColumn.list.Add(new SlotElementData(SlotElementType.FinalExplosion));
        Debug.Log("AddExtrasCmd: explosion added to column " + randColumn.columnIndex);
    }

    private void AddBicksMultiplier(CityElementDataContainer dataContainer)
    {
        var hasMult = dataContainer.columns.Any(s => s.list.Any(e => e.type == SlotElementType.AddMoreBricks));
        if (hasMult)
        {
            return;
        }
        var columnsWithBricks = dataContainer.columns.Where(c => c.list.All(e => e.type == SlotElementType.Bricks)).ToList();
        if (columnsWithBricks.Count == 0)
        {
            return;
        }
        var randColumn = RandHelper.GetRandomElement(columnsWithBricks);
        var prevLength = randColumn.list.Count;
        if (prevLength <= 4)
        {
            return;
        }
        var randIndex = Random.Range(1, randColumn.list.Count - 2);
        //var randIndex = 0;
        randColumn.list.Insert(randIndex, new SlotElementData(SlotElementType.AddMoreBricks));
        Assert.AreEqual(prevLength + 1, randColumn.list.Count, "UnlockCityElementCmd AddBicksMultiplier: failed to add additional bricks multiplier to column");
    }

    private void AddCoinsIfAbsent(CityElementDataContainer dataContainer)
    {
        var hasCoins = dataContainer.columns.Any(s => s.list.Any(e => e.type == SlotElementType.Coins));
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
        var randIndex = Random.Range(2, randColumn.list.Count - 1);
        randColumn.list.Insert(randIndex, new SlotElementData(SlotElementType.Coins));
    }

    private void AddAd(CityElementDataContainer dataContainer)
    {
        var hasAd = dataContainer.columns.Any(s => s.list.Any(e => e.type == SlotElementType.Ad));
        if (hasAd)
        {
            return;
        }
        var totalBricks = dataContainer.columns.Sum(c => c.list.Count(e => e.type == SlotElementType.Bricks));
        if (totalBricks <= 10)
        {
            //small element
            return;
        }
        var randColumn = RandHelper.GetRandomElement(dataContainer.columns);
        var prevLength = randColumn.list.Count;
        if (prevLength <= 4)
        {
            return;
        }
        var randIndex = Random.Range(2, randColumn.list.Count - 1);
        //var randIndex = 0;
        randColumn.list.Insert(randIndex, new SlotElementData(SlotElementType.Ad));
        Assert.AreEqual(prevLength + 1, randColumn.list.Count, "UnlockCityElementCmd AddAd: failed to add ad to column");
    }

}