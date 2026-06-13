using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class ModelUtils
{
    public static CityElement GetCurrentElement()
    {
        var currentElementData = PlayerModel.Instance.playerData.currentElement;
        if (currentElementData == null)
        {
            return null;
        }
        var cityElement = CityModel.Instance.GetElementByDataKey(currentElementData.dataKey);
        Assert.IsNotNull(cityElement, "ModelUtils GetCurrentElement: no city element found for data key " + currentElementData.dataKey);
        return cityElement;
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

    public static bool CurrentGroupCompleted()
    {
        var currentGroup = CityModel.Instance.GetGroupByName(PlayerModel.Instance.playerData.currentGroupName);
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
        var currentGroupName = PlayerModel.Instance.playerData.currentGroupName;
        return CityModel.Instance.GetAllGroupNames().FindIndex(g => g == currentGroupName);
    }



}