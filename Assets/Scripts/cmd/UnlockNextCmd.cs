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

    private readonly int firstCityElementIndex = 0;


    public UnlockNextCmd()
    {
        currentElementData = playerModel.playerData.currentElement;
        currentGroupName = playerModel.playerData.currentGroupName;
        Assert.IsFalse(string.IsNullOrEmpty(currentGroupName), "UnlockCityElementCmd: currentGroup should not be null or empty");
    }

    public void Run()
    {

        playerModel.playerData.isDirty = true;
        this.currentGroupData = balancingModel.GetDataCopy(this.currentGroupName);

        if (currentElementData == null)
        {
            var firstElementName = cityModel.GetElementByIndex(firstCityElementIndex).dataKey;
            currentElementData = currentGroupData.cityElementDataList.Find(e => e.dataKey == firstElementName);
            UnlockElement(currentElementData);
            return;
        }

        Assert.IsTrue(currentElementData.ElementCompleted(), "UnlockCityElementCmd Run: current element not completed");
        Assert.IsTrue(currentElementData.AllSlotsEmpty(), "UnlockCityElementCmd Run: current element still has bricks or coins in slots");


        var currentGroupElement = CityModel.Instance.GetGroupByName(this.currentGroupName);
        var elementsInGroup = currentGroupElement.GetElements().ToList();
        var elementIndexInGroup = elementsInGroup.FindIndex((e) => e.dataKey == currentElementData.dataKey);
        var nextElementIndexInGroup = elementIndexInGroup + 1;


        Debug.Log("UnlockNextCmd Run nextElementIndexInGroup " + nextElementIndexInGroup);

        if (nextElementIndexInGroup >= elementsInGroup.Count)
        {
            var nextGroupName = cityModel.GetNextGroupName();
            if (nextGroupName == null)
            {
                //out of groups, game completed.
                Debug.LogError("UnlockNextCmd no groups");
            }
            else
            {
                cityModel.SetCurrentGroupName(nextGroupName);
                currentElementData = null;
                currentGroupName = nextGroupName;
                playerModel.playerData.currentGroupName = nextGroupName;
                playerModel.playerData.currentElement = null;
                this.Run();
            }
            return;
        }
        var nextElement = elementsInGroup[nextElementIndexInGroup];
        var nextElementData = currentGroupData.cityElementDataList.Find((e) => e.dataKey == nextElement.dataKey);
        UnlockElement(nextElementData);
    }

    private void UnlockElement(CityElementDataContainer cityElementData)
    {
        Assert.IsNotNull(cityElementData, "UnlockCityElementCmd UnlockElement: cityElementData should not be null");
        Assert.IsFalse(cityElementData.ElementCompleted(), "UnlockCityElementCmd UnlockElement: city element should not be completed to be unlocked. " + cityElementData.dataKey);
        Assert.IsTrue(cityElementData.columns.Count > 1, "UnlockCityElementCmd UnlockElement: city element should have at least 2 columns to be unlocked. " + cityElementData.dataKey);

        AddCoinsIfAbsent(cityElementData);
        if (cityElementData.ElementCountColoredBricks() == 0)
            cityElementData.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement);
        var cityElement = cityModel.GetElementByDataKey(cityElementData.dataKey);

        var elementIndex = cityModel.GetElementIndex(cityElement);
        cityModel.ActivateElements(elementIndex);

        cityElement.Setup(cityElementData);
        slotModel.Fill(cityElementData.columns);
        this.MoveCam(cityElement);

        playerModel.playerData.currentElement = cityElementData;
        playerModel.playerData.isDirty = true;


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

    private void MoveCam(CityElement cityElement)
    {
        new MovCamCmd().Run(cityElement);
    }

}