using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class UnlockNextCmd
{
    private PlayerModel playerModel => PlayerModel.Instance;
    private SlotModel slotModel => SlotModel.Instance;
    private CityModel cityModel => CityModel.Instance;
    private BalancingModel balancingModel => BalancingModel.Instance;

    private CityElementDataContainer currentElementData;
    private string currentGroupName;
    private GroupDataList currentGroupData;

    private readonly int firstCityElementIndex = BalancingModel.FirstCityElementIndex;


    public UnlockNextCmd()
    {
        var progress = playerModel.playerData.GetCurrentGroupProgress();
        currentElementData = progress.currentElement;
        currentGroupName = progress.groupName;
        Assert.AreNotEqual(progress.state, GroupState.Completed, "UnlockNextCmd: current group is already completed");
    }

    public void Run()
    {
        Debug.Log("UnlockNextCmd Run with currentGroupName " + currentGroupName + ", currentElement " + (currentElementData != null ? currentElementData.dataKey : "null"));
        this.currentGroupData = balancingModel.GetDataCopy(this.currentGroupName);

        if (currentElementData == null)
        {
            var firstElementName = cityModel.GetElementByIndex(firstCityElementIndex).dataKey;
            currentElementData = currentGroupData.cityElementDataList.Find(e => e.dataKey == firstElementName);
            UnlockElement(currentElementData);
            return;
        }

        if (currentElementData.ElementCompleted())
        {
            var currentGroupElement = CityModel.Instance.GetGroupByName(this.currentGroupName);
            var elementsInGroup = currentGroupElement.GetElements().ToList();
            var elementIndexInGroup = elementsInGroup.FindIndex((e) => e.dataKey == currentElementData.dataKey);
            var nextElementIndexInGroup = elementIndexInGroup + 1;

            if (nextElementIndexInGroup >= elementsInGroup.Count)
            {
                //no more elements in group.
                //new CompleteCurrentGroupCmd().Run();
                return;
            }
            var nextElement = elementsInGroup[nextElementIndexInGroup];
            var nextElementData = currentGroupData.cityElementDataList.Find((e) => e.dataKey == nextElement.dataKey);
            UnlockElement(nextElementData);
            return;
        }

        UnlockElement(currentElementData);
    }


    private void UnlockElement(CityElementDataContainer cityElementData)
    {
        Debug.Log("UnlockNextCmd UnlockElement with city element data " + cityElementData.dataKey);
        Assert.IsNotNull(cityElementData, "UnlockCityElementCmd UnlockElement: cityElementData should not be null");
        //Assert.IsFalse(cityElementData.ElementCompleted(), "UnlockCityElementCmd UnlockElement: city element should not be completed to be unlocked. " + cityElementData.dataKey);
        Assert.IsTrue(cityElementData.columns.Count > 0, "UnlockCityElementCmd UnlockElement: city element should have at least 1 column to be unlocked. " + cityElementData.dataKey);

        AddCoinsIfAbsent(cityElementData);
        AddBicksMultiplier(cityElementData);
        AddExplosion(cityElementData);
        if (cityElementData.ElementCountColoredBricks() == 0)
            cityElementData.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement + 1);
        var cityElement = cityModel.GetElementByDataKey(cityElementData.dataKey);

        var elementIndex = cityModel.GetElementIndex(cityElement);
        cityModel.ActivateElements(elementIndex);

        cityElement.Setup(cityElementData);
        slotModel.Fill(cityElementData.columns);
        this.MoveCam(cityElement);

        playerModel.playerData.currentElement = cityElementData;
        playerModel.playerData.isDirty = true;

        PlayerModel.Instance.OnPlayerDataChanged?.Invoke();
    }

    private void AddExplosion(CityElementDataContainer dataContainer)
    {
        var hasExplosion = dataContainer.columns.Any(s => s.list.Any(e => e.type == SlotElementType.Explosion));
        if (hasExplosion)
        {
            return;
        }
        var columnsWithBricks = dataContainer.columns.Where(c => c.list.All(e => e.type == SlotElementType.Bricks)).ToList();
        if (columnsWithBricks.Count == 0)
        {
            return;
        }
        var randColumn = RandHelper.GetRandomElement(columnsWithBricks);
        randColumn.list.Add(new SlotElementData(SlotElementType.Explosion));
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
        if (prevLength <= 2)
        {
            return;
        }
        //var randIndex = Random.Range(1, randColumn.list.Count - 2);
        var randIndex = 0;
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
        //Debug.Log("UnlockCityElementCmd AddCoins for element " + dataContainer.dataKey + ", random column: " + randColumn.columnIndex);
        randColumn.list.Add(new SlotElementData { type = SlotElementType.Coins });
    }

    private void AddAd(CityElementDataContainer dataContainer)
    {
        var hasAd = dataContainer.columns.Any(s => s.list.Any(e => e.type == SlotElementType.Ad));
        if (hasAd)
        {
            return;
        }
        var columnsWithBricks = dataContainer.columns.Where(c => c.list.All(e => e.type == SlotElementType.Bricks)).ToList();
        var randColumn = RandHelper.GetRandomElement(columnsWithBricks);
        var prevLength = randColumn.list.Count;
        if (prevLength <= 2)
        {
            return;
        }
        //var randIndex = Random.Range(1, randColumn.list.Count - 2);
        var randIndex = 0;
        randColumn.list.Insert(randIndex, new SlotElementData(SlotElementType.Ad));
        Assert.AreEqual(prevLength + 1, randColumn.list.Count, "UnlockCityElementCmd AddAd: failed to add ad to column");
    }

    private void MoveCam(CityElement cityElement)
    {
        CamModel.Instance.MoveCameraToCityElement(cityElement);
    }

}