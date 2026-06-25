using System;
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
    private int difficultyToApply;
    private NonRepeatingShuffleBag<SlotColumnData> columns;
    private Dictionary<SlotColumnData, HashSet<int>> randIndex;

    private int maxCoins = 0;
    private int maxHiddenBricks = 0;
    private int maxDeaths = 0;
    private int maxAds = 0;
    private int maxMults = 0;

    private static readonly int[] difficulties = new int[] { 0, 1, 2, 3, 4, 5, 6, 2 };

    private BuildingName buildingName;
    public AddExtrasCmd(BuildingName buildingName)
    {
        this.buildingName = buildingName;
    }

    public void Run(CityElementDataContainer currentElementData, int difficultyToApply = -1)
    {
        Assert.IsNotNull(currentElementData, "AddExtrasCmd Run: data container is null");
        Assert.IsTrue(currentElementData.columns.Count > 0, "AddExtrasCmd Run: data container should have at least 1 column");
        var totalBricks = currentElementData.columns.Sum(c => c.list.Count(e => e.IsBrick));
        Assert.IsTrue(totalBricks > 0, "AddExtrasCmd Run: data container should have at least 1 brick to add extras");
        this.dataContainer = currentElementData;

        var extrasApplied = currentElementData.columns.Any(c => c.list.Any(e => !e.IsBrick));
        if (extrasApplied || IsShort())
        {
            return;
        }
        this.SetLimits(totalBricks);

        this.columns = new NonRepeatingShuffleBag<SlotColumnData>(this.dataContainer.columns);
        this.randIndex = new Dictionary<SlotColumnData, HashSet<int>>();

        var amountOfExtrasPerColumn = Mathf.RoundToInt(this.SumLimits() / (float)this.dataContainer.columns.Count);
        Assert.IsTrue(amountOfExtrasPerColumn > 0, "invalid amountOfExtrasPerColumn");

        for (var i = 0; i < this.dataContainer.columns.Count; i++)
        {
            var c = this.dataContainer.columns[i];
            var maxElementInList = c.list.Count - 1;//-1 will make sure the last element is never adressed. we dont need the last element to be coin or death or anything.
            this.randIndex[c] = RandHelper.GetRandIndexList(maxElementInList, amountOfExtrasPerColumn);
        }

        if (difficultyToApply == -1)
        {
            var pd = PlayerModel.Instance.playerData;
            pd.difficultyIndex++;
            if (pd.difficultyIndex >= difficulties.Length)
            {
                pd.difficultyIndex = 0;
            }
            pd.isDirty = true;
            this.difficultyToApply = difficulties[pd.difficultyIndex];
        }
        else
        {
            this.difficultyToApply = difficultyToApply;
        }

        ApplyDifficulty();
        // AddExplosion();
    }

    private int SumLimits()
    {
        return this.maxCoins + this.maxHiddenBricks + this.maxDeaths + this.maxMults + this.maxAds;
    }

    private void SetLimits(int totalBricks)
    {
        if (totalBricks < 20)
        {
            this.maxCoins = 1;
            this.maxHiddenBricks = 1;
            this.maxDeaths = 1;
            this.maxMults = 1;
            this.maxAds = 1;
        }
        else
        {
            this.maxCoins = Mathf.RoundToInt(totalBricks / 40.0f);
            if (this.maxCoins < 1)
            {
                this.maxCoins = 1;
            }
            this.maxHiddenBricks = Mathf.RoundToInt(totalBricks / 15.0f);
            this.maxDeaths = Mathf.RoundToInt(totalBricks / 25.0f);
            this.maxMults = Mathf.RoundToInt(totalBricks / 20.0f);
            this.maxAds = Mathf.RoundToInt(totalBricks / 30.0f);
            if (this.maxAds < 1)
            {
                this.maxAds = 1;
            }
        }

        if (this.ShouldAddAd() == false)
        {
            this.maxAds = 0;
        }
        if (coins >= RemoteConfigModel.Instance.RemoteConfig.RefillCoins)
        {
            this.maxCoins--;
        }
    }

    private bool IsShort()
    {
        foreach (var c in dataContainer.columns)
        {
            var n = c.list.Count();
            if (n < 3)
            {
                return true;
            }
        }
        return false;
    }

    private bool ShouldAddAd()
    {
        var hasGoldenTicket = IAPModel.Instance.DidPurchaseComplete(IAPProductName.GoldenTicket) || IAPModel.Instance.HasTempGoldenTicket();
        if (hasGoldenTicket)
        {
            //don't add ad if player has golden ticket
            return false;
        }

        if (BuildingNameUtil.IsPremiumBuilding(this.buildingName))
        {
            //don't add ad for premium building
            return false;
        }

        var remoteConfig = RemoteConfigModel.Instance.RemoteConfig;
        var curTimestamp = TimeUtils.GetUnixTimestamp();
        var installTimestamp = PlayerModel.Instance.playerData.installTimestamp;
        var secPassedSinceInstall = curTimestamp - installTimestamp;
        if (secPassedSinceInstall < remoteConfig.ShowMidSessionAdAfterSec)
        {
            //don't add ad for the first 5 minutes after install
            return false;
        }
        var lastInterstitialTimestamp = AdModel.Instance.LastInterstitialTimestamp;
        if (lastInterstitialTimestamp > 0)
        {
            var timeDiff = curTimestamp - lastInterstitialTimestamp;
            if (timeDiff < remoteConfig.ShowMidSessionAdAfterSec)
            {
                //don't add ad if the last interstitial was shown less than configured seconds ago
                return false;
            }
        }
        var didLoadAd = AdModel.Instance.IsAdReady(RewardName.MID_SESSION_INTERSTITIAL) || AdModel.Instance.IsAdReady(RewardName.MID_SESSION_REWARDED);
        return didLoadAd;
    }

    private void IncrementRandIndexes(SlotColumnData column, int startAt)
    {
        var result = new HashSet<int>();
        var list = this.randIndex[column];
        if (list.Count == 0)
        {
            return;
        }
        for (var i = startAt; i < list.Count; i++)
        {
            result.Add(list.ElementAt(i) + 1);
        }
        this.randIndex[column] = result;
    }

    private void AddCoins()
    {
        while (maxCoins > 0)
        {
            var column = this.columns.GetNext();
            var randIndexHashSet = this.randIndex[column];
            if (randIndexHashSet.Count == 0)
            {
                //should not happen but just in case.
                maxCoins--;
                continue;
            }
            var randPos = RandHelper.GetRandomInt(0, randIndexHashSet.Count);
            var value = randIndexHashSet.ElementAt(randPos);
            randIndexHashSet.Remove(value);
            if (AddCoins(column, value))
                IncrementRandIndexes(column, randPos);
            maxCoins--;
        }
    }

    private void AddAds()
    {
        while (maxAds > 0)
        {
            var column = this.columns.GetNext();
            var randIndexHashSet = this.randIndex[column];
            if (randIndexHashSet.Count == 0)
            {
                //should not happen but just in case.
                maxAds--;
                continue;
            }
            var randPos = RandHelper.GetRandomInt(0, randIndexHashSet.Count);
            var value = randIndexHashSet.ElementAt(randPos);
            randIndexHashSet.Remove(value);
            if (AddAd(column, value))
                IncrementRandIndexes(column, randPos);
            maxAds--;
        }
    }

    private void AddBicksMultiplier()
    {
        while (maxMults > 0)
        {
            var column = this.columns.GetNext();
            var randIndexHashSet = this.randIndex[column];
            if (randIndexHashSet.Count == 0)
            {
                //should not happen but just in case.
                maxMults--;
                continue;
            }
            var randPos = RandHelper.GetRandomInt(0, randIndexHashSet.Count);
            var value = randIndexHashSet.ElementAt(randPos);
            randIndexHashSet.Remove(value);
            if (AddBicksMultiplier(column, value))
                IncrementRandIndexes(column, randPos);
            maxMults--;
        }
    }

    private void SetHiddenBricks()
    {
        while (maxHiddenBricks > 0)
        {
            var column = this.columns.GetNext();
            var randIndexHashSet = this.randIndex[column];
            if (randIndexHashSet.Count == 0)
            {
                //should not happen but just in case.
                maxHiddenBricks--;
                continue;
            }
            var randPos = RandHelper.GetRandomInt(0, randIndexHashSet.Count);
            var value = randIndexHashSet.ElementAt(randPos);
            randIndexHashSet.Remove(value);
            SetHiddenBrick(column, value);
            maxHiddenBricks--;
        }
    }


    private void AddDeaths()
    {
        while (maxDeaths > 0)
        {
            var column = this.columns.GetNext();
            var randIndexHashSet = this.randIndex[column];
            if (randIndexHashSet.Count == 0)
            {
                //should not happen but just in case.
                maxDeaths--;
                continue;
            }
            var randPos = RandHelper.GetRandomInt(0, randIndexHashSet.Count);
            var value = randIndexHashSet.ElementAt(randPos);
            randIndexHashSet.Remove(value);
            if (AddDeath(column, value))
                IncrementRandIndexes(column, randPos);
            maxDeaths--;
        }
    }

    private void ApplyDifficulty()
    {
        if (difficultyToApply == 0)
        {
            AddCoins();
            return;
        }

        if (difficultyToApply == 1)
        {
            AddAds();
            AddBicksMultiplier();
            return;
        }
        if (difficultyToApply == 2)
        {
            AddCoins();
            SetHiddenBricks();
            return;
        }
        if (difficultyToApply == 3)
        {
            AddAds();
            AddDeaths();
            AddBicksMultiplier();
            return;
        }
        if (difficultyToApply == 4)
        {
            AddCoins();
            SetHiddenBricks();
            AddDeaths();
            return;
        }
        if (difficultyToApply == 5)
        {
            AddAds();
            SetHiddenBricks();
            AddDeaths();
            AddBicksMultiplier();
            return;
        }
        if (difficultyToApply == 6)
        {
            AddCoins();
            SetHiddenBricks();
            AddDeaths();
            return;
        }

    }


    private void SetHiddenBrick(SlotColumnData column, int atIndex)
    {
        var nextBrickIndex = column.GetNextBrickIndex(atIndex);
        if (nextBrickIndex != -1)
        {
            column.list[nextBrickIndex].type = SlotElementType.HiddenBricks;
        }
    }

    private static NonRepeatingShuffleBag<int> deadCounter = new NonRepeatingShuffleBag<int>(new List<int> { 1, 1, 1, 2, 2, 3 });

    private bool AddDeath(SlotColumnData column, int atIndex)
    {
        if (atIndex >= 0 && atIndex < column.list.Count && column.list[atIndex].type != SlotElementType.EmitterDeathWaiting)
        {
            column.list.Insert(atIndex, new SlotElementData(SlotElementType.EmitterDeathWaiting)
            {
                deadCounter = deadCounter.GetNext()
            });
            return true;
        }
        return false;
    }

    private bool AddBicksMultiplier(SlotColumnData column, int atIndex)
    {
        return Add(column, atIndex, SlotElementType.AddMoreBricks);
    }

    private bool AddCoins(SlotColumnData column, int atIndex)
    {
        return Add(column, atIndex, SlotElementType.Coins);
    }

    private bool AddAd(SlotColumnData randColumn, int atIndex)
    {
        return Add(randColumn, atIndex, SlotElementType.Ad);
    }

    private bool Add(SlotColumnData column, int atIndex, SlotElementType set)
    {
        if (atIndex >= 0 && atIndex < column.list.Count && column.list[atIndex].type != set)
        {
            column.list.Insert(atIndex, new SlotElementData(set));
            return true;
        }
        return false;

    }


}