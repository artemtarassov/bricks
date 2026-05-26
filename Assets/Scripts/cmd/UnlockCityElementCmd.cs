using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class UnlockCityElementCmd
{
    private PlayerModel playerModel => PlayerModel.Instance;
    private SlotModel slotModel => SlotModel.Instance;
    private CityModel cityModel => CityModel.Instance;
    private BalancingModel balancingModel => BalancingModel.Instance;

    private GroupDataList currentGroup;

    public UnlockCityElementCmd()
    {
        var c = playerModel.playerData.currentGroup;
        Assert.IsNotNull(c, "UnlockCityElementCmd: currentGroup should not be null");
        currentGroup = c;
    }

    public void Run()
    {
        var hasBricks = currentGroup.HasBricks();
        if (!hasBricks)
        {
            //next group.
            var nextGroupName = balancingModel.GetNextGroup(currentGroup.groupName);
            cityModel.SetCurrentGroupName(nextGroupName);
            this.currentGroup = balancingModel.GetDataCopy(nextGroupName);
            playerModel.playerData.currentGroup = this.currentGroup;
            playerModel.playerData.isDirty = true;
            this.NextElement();
            return;
        }
        var firstCityElementWithBricks = currentGroup.cityElementDataList.FirstOrDefault(e => e.SlotsHaveBricks());
        Assert.IsNotNull(firstCityElementWithBricks, "UnlockCityElementCmd: no city element with bricks found in current group");

        firstCityElementWithBricks.EnableDifferentColors(BalancingModel.AdditionalBricksOnEmptyElement);

        var cityElement = cityModel.UnlockElement(firstCityElementWithBricks.dataKey);
        cityElement.Setup(firstCityElementWithBricks);
        slotModel.Fill(firstCityElementWithBricks.slotElementDataList);

        this.MoveCam(cityElement);
    }


    private void NextElement()
    {
        var cityElement = CityModel.Instance.UnlockNextElement();
        var dataContainer = BalancingModel.Instance.GetDataCopy(currentGroup.groupName, cityElement.dataKey);
        cityElement.Setup(dataContainer);
        slotModel.Fill(dataContainer.slotElementDataList);

        this.MoveCam(cityElement);
    }

    private void MoveCam(CityElement cityElement)
    {
        var mainCam = Camera.main;
        if (cityElement.camPos == Vector3.zero || cityElement.camRot == Vector3.zero)
        {
            var p = cityElement.GetAveragePosition();
            mainCam.transform.DOMove(p + new Vector3(20, 10, 20), 1f).OnUpdate(() =>
            {
                mainCam.transform.LookAt(p);
            });
        }
        else
        {
            mainCam.transform.DOMove(cityElement.camPos, 1f);
            mainCam.transform.DORotate(cityElement.camRot + new Vector3(5, 0, 0), 1f);
        }
    }

}