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
        var cityElement = CityModel.Instance.UnlockNextElement();
        new SetNextBricksColorsCmd(cityElement).Run();
        this.SetupBricksInSlots(cityElement);
    }

    private void SetupBricksInSlots(CityElement cityElement)
    {
        var originPredefinedBricks = cityElement.cityElementColors.predefinedBricks;
        var predefinedBricks = originPredefinedBricks.ToList();
        var predefinedBricksCount = predefinedBricks.Sum((a) => a.amount);
        Assert.IsTrue(predefinedBricks.Count > 0, "predefinedBricks is empty");

        var sm = SlotModel.Instance;
        sm.ClearAll();
        var maxColumns = sm.Columns.Count;

        while (predefinedBricks.Count > 0)
        {
            for (var c = 0; c < maxColumns && predefinedBricks.Count > 0; c++)
            {
                var b = predefinedBricks.First().Clone();
                predefinedBricks.RemoveAt(0);
                sm.AddBricksToColumn(c, b);
            }
            var randColumn = Random.Range(0, maxColumns);
            sm.AddMoreBricksToColumn(randColumn);
        }
        sm.OnSlotsChanged?.Invoke(null);



        var bricksInSlots = sm.CountBricks();
        var allBricks = cityElement.CountBricks();
        Assert.AreEqual(allBricks, predefinedBricksCount, "allBricks not equal CountBricks. allBricks " + allBricks + " predefinedBricksCount " + predefinedBricksCount);

        Assert.IsTrue(bricksInSlots > 0, "There should be bricks in slots after unlocking city element");
        Assert.AreEqual(allBricks, bricksInSlots, "invalid count");


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