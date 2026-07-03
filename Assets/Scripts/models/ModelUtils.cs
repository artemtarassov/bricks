using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class ModelUtils
{
    public static CityElement GetCurrentElement()
    {
        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        var currentElementData = progress.GetCurrentElement();
        if (currentElementData == null)
        {
            return null;
        }
        var cityElement = CityModel.Instance.GetElementByDataKey(currentElementData.dataKey);
        Assert.IsNotNull(cityElement, "ModelUtils GetCurrentElement: no city element found for data key " + currentElementData.dataKey);
        return cityElement;
    }

    public static GameOverReason IsGameOver()
    {
        var element = GetCurrentElement();
        if (element.dataContainer.ElementCompleted())
        {
            return GameOverReason.Undefined;
        }
        if (!SlotModel.Instance.HasEmitterSpace() && element.dataContainer.ElementCountEmittingBricks() == 0)
        {
            return GameOverReason.OutOfSpace;
        }
        if (element.dataContainer.IsOutOfTime())
        {
            return GameOverReason.OutOfTime;
        }
        return GameOverReason.Undefined;
    }

    public static bool CurrentBuildingCompleted()
    {
        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        var building = CityModel.Instance.GetBuildingByName(progress.BuildingName);
        var isLastElement = progress.currentElementIndex >= building.GetElements().Count - 1;
        if(isLastElement && progress.GetCurrentElement().ElementCompleted())
        {
            return true;
        }
        return false;
    }

    public static int GetCurrentBuildingIndex()
    {
        var bn = PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName;
        return BuildingNameUtil.GetAllBuildingNames(true).FindIndex(b => b == bn);
    }



}