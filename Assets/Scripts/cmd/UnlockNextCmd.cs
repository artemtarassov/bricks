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
    private BuildingName currentBuildingName;
    private BuildingData currentGroupData;

    private readonly int firstCityElementIndex = BalancingModel.FirstCityElementIndex;

    private BuildingProgressData progress => playerModel.playerData.GetCurrentBuildingProgress();

    private CityModel cm => CityModel.Instance;


    public UnlockNextCmd()
    {
        currentElementData = progress.GetCurrentElement();
        currentBuildingName = progress.BuildingName;
    }

    public void Run()
    {
        Debug.Log("UnlockNextCmd Run with currentBuildingName " + currentBuildingName + ", currentElement " + (currentElementData != null ? currentElementData.dataKey : "null"));

        this.currentGroupData = balancingModel.GetDataCopy(this.currentBuildingName);
        ViewModel.Instance.ResetOutOfSpaceCounter();

        if (currentElementData == null)
        {
            var firstElementName = cityModel.GetElementByIndex(firstCityElementIndex).dataKey;
            currentElementData = currentGroupData.cityElementDataList.Find(e => e.dataKey == firstElementName);
            progress.ResetElementsCounter();
            progress.SetCurrentElement(currentElementData);
        }
        else
            if (currentElementData.ElementCompleted())
            {
                var currentBuilding = cm.GetBuildingByName(this.currentBuildingName);
                var elementsInBuilding = currentBuilding.GetElements().ToList();
                var elementIndexInBuilding = elementsInBuilding.FindIndex((e) => e.dataKey == currentElementData.dataKey);
                var nextElementIndexInBuilding = elementIndexInBuilding + 1;

                {
                    progress.IncCompletedElementsCounter();
                    var completedElementsCounter = progress.CompletedElementsCounter;
                    var buildingIndex = cm.GetBuildingNameIndex(currentBuildingName);
                    var dict = new Dictionary<string, object>();
                    dict["completedElementsCounter"] = completedElementsCounter;
                    dict["elementName"] = currentElementData.dataKey;
                    dict["themeIndex"] = buildingIndex;
                    new LogEventCmd().Run("complete_element", dict);
                }
                if (nextElementIndexInBuilding >= elementsInBuilding.Count)
                {
                    //no more elements in building.
                    //new CompleteCurrentBuildingCmd().Run();
                    return;
                }
                var nextElement = elementsInBuilding[nextElementIndexInBuilding];
                var nextElementData = currentGroupData.cityElementDataList.Find((e) => e.dataKey == nextElement.dataKey);
                progress.SetCurrentElement(nextElementData);
                currentElementData = nextElementData;
            }

        new AddExtrasCmd().Run(currentElementData);
        UnlockElement(currentElementData);
        playerModel.OnPlayerDataChanged?.Invoke();
        new SoundCmd(SoundModel.Instance.MUSIC1).Run();
    }


    private void UnlockElement(CityElementDataContainer currentElementData)
    {
        Debug.Log("UnlockNextCmd UnlockElement with city element data " + currentElementData.dataKey);
        Assert.IsNotNull(currentElementData, "UnlockCityElementCmd UnlockElement: cityElementData should not be null");
        Assert.IsTrue(currentElementData.columns.Count > 0, "UnlockCityElementCmd UnlockElement: city element should have at least 1 column to be unlocked. " + currentElementData.dataKey);

        var cityElement = cityModel.GetElementByDataKey(currentElementData.dataKey);
        Assert.IsNotNull(cityElement, $"UnlockNextCmd UnlockElement: failed to find city element with dataKey {currentElementData.dataKey}");
        var elementIndex = cityModel.GetElementIndex(cityElement);
        cityModel.ActivateElements(elementIndex);

        cityElement.Setup(currentElementData);
        slotModel.Fill(currentElementData.columns);

        ViewModel.Instance.ChangeBottomNav(BottomNav.Slots);


#if UNITY_EDITOR
        ValidateAmounts(cityElement, currentElementData);
#endif

        if (currentElementData.ElementCountColoredBricks() == 0)
        {
            CityModel.Instance.EnableDifferentColors(cityElement, BalancingModel.AdditionalBricksOnEmptyElement);
        }

        this.MoveCam(cityElement);
    }

    private void ValidateAmounts(CityElement cityElement, CityElementDataContainer currentElementData)
    {
        /*
        var bricksInSlots = currentElementData.columns.FindAll(c => c.list.Any(e => e.brickData != null)).SelectMany(c => c.list).Where(e => e.brickData != null).Select(e => e.brickData);
        var bricksInElement = currentElementData.brickDataList;

        var sum1 = bricksInSlots.Sum(b => b.max);
        var sum2 = bricksInElement.Sum(b => b.max);
        Debug.Log("UnlockNextCmd ValidateAmounts: sum of colored bricks in slots " + sum1 + ", sum of colored bricks in element data " + sum2);
        Assert.IsTrue(sum1 > 0, "UnlockNextCmd ValidateAmounts: total amount of colored bricks in slots should be greater than 0");
        Assert.IsTrue(sum2 > 0, "UnlockNextCmd ValidateAmounts: total amount of colored bricks in element data should be greater than 0");
        Assert.AreEqual(bricksInSlots.Count(), bricksInElement.Count(), "UnlockNextCmd ValidateAmounts: number of brick entries in slots should be equal to number of brick entries in element data. slots count: " + bricksInSlots.Count() + ", element data count: " + bricksInElement.Count());
        Assert.AreEqual(sum1, sum2, "UnlockNextCmd ValidateAmounts: total amount of colored bricks in slots should be equal to total amount of colored bricks in element data. sum1: " + sum1 + ", sum2: " + sum2);
    */
    }



    private void MoveCam(CityElement cityElement)
    {
        CamModel.Instance.MoveCameraToCityElement(cityElement);
        new SoundCmd(SoundModel.Instance.CAM_MOVE_BACK).Run();

#if UNITY_EDITOR
        //select gameobject
        var go = cityElement.gameObject;
        UnityEditor.Selection.activeGameObject = go;
#endif
    }

}