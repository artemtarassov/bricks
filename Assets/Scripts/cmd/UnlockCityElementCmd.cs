using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class UnlockCityElementCmd
{
    public UnlockCityElementCmd()
    {
    }

    public void Run()
    {
        Assert.IsTrue(BalancingModel.Instance.DidLoad(), "BalancingModel data not loaded. Did you forget to call Load()?");
        var cityElement = CityModel.Instance.UnlockNextElement();
        cityElement.dataContainer = BalancingModel.Instance.GetData(cityElement.dataKey);
        cityElement.ShowNextColoredBricks(BalancingModel.AdditionalBricksOnEmptyElement);

        var sm = SlotModel.Instance;
        sm.ClearAll();
        var slotElementDataList = cityElement.dataContainer.slotElementDataList;
        foreach (var sedl in slotElementDataList)
        {
            var columnIndex = sedl.columnIndex;
            foreach (var sed in sedl.list)
            {
                if (sed.type == SlotElementType.Bricks)
                {
                    sm.AddBricksToColumn(columnIndex, sed.brickData);
                }
                else if (sed.type == SlotElementType.AddMoreBricks)
                {
                    sm.AddMoreBricksToColumn(columnIndex);
                }
            }
        }

        sm.OnSlotsChanged?.Invoke(null);


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