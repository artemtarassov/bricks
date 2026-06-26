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

    public static bool IsOutOfTime()
    {
        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        if (!BuildingNameUtil.IsChallengeBuilding(progress.BuildingName))
        {
            return false;
        }
        return ChallengeModel.Instance.GetSecondsLeft() == 0;
    }

    public static bool IsOutOfSpace()
    {
        var element = GetCurrentElement();
        if (!SlotModel.Instance.HasEmitterSpace() && element.dataContainer.ElementCountEmittingBricks() == 0)
        {
            return true;
        }
        return false;
    }

    public static bool CurrentBuildingCompleted()
    {
        var currentGroupName = PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName;
        var currentGroup = CityModel.Instance.GetBuildingByName(currentGroupName);
        var cityElements = currentGroup.GetElements();
        foreach (var cityElement in cityElements)
        {
            if (!cityElement.gameObject.activeSelf)
            {
                return false;
            }
            var elementDidSetup = cityElement.dataContainer != null;
            if (elementDidSetup)
            {
                if (!cityElement.dataContainer.ElementCompleted())
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static int GetCurrentGroupIndex()
    {
        var bn = PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName;
        return CityModel.Instance.GetBuildingNameIndex(bn);
    }



}