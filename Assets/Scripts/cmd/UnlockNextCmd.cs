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

    private GroupProgressData progress => playerModel.playerData.GetCurrentGroupProgress();


    public UnlockNextCmd()
    {
        currentElementData = playerModel.playerData.currentElement;
        currentGroupName = playerModel.playerData.currentGroupName;
    }

    public void Run()
    {
        Debug.Log("UnlockNextCmd Run with currentGroupName " + currentGroupName + ", currentElement " + (currentElementData != null ? currentElementData.dataKey : "null"));

        this.currentGroupData = balancingModel.GetDataCopy(this.currentGroupName);

        if (currentElementData == null)
        {
            var firstElementName = cityModel.GetElementByIndex(firstCityElementIndex).dataKey;
            currentElementData = currentGroupData.cityElementDataList.Find(e => e.dataKey == firstElementName);
            this.playerModel.playerData.currentElement = currentElementData;
        }
        else
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
                this.playerModel.playerData.currentElement = nextElementData;
                currentElementData = nextElementData;
                progress.completedElementsCounter++;
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

        if (currentElementData.ElementCountColoredBricks() == 0)
            currentElementData.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement + 1);
        var cityElement = cityModel.GetElementByDataKey(currentElementData.dataKey);

        var elementIndex = cityModel.GetElementIndex(cityElement);
        cityModel.ActivateElements(elementIndex);

        cityElement.Setup(currentElementData);
        slotModel.Fill(currentElementData.columns);
        this.MoveCam(cityElement);
    }



    private void MoveCam(CityElement cityElement)
    {
        CamModel.Instance.MoveCameraToCityElement(cityElement);
        new SoundCmd(SoundModel.Instance.CAM_MOVE_BACK).Run();
    }

}