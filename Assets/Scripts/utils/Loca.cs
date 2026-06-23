using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class Loca
{
    public static string GetThemeName(BuildingName buildingName)
    {
        switch (buildingName)
        {
            case BuildingName.Preset_Bath_House_01:
                return "Baiae";
            case BuildingName.Ruins1_House:
                return "Palermo";
            case BuildingName.Tower_House:
                return "Veneto";
            case BuildingName.Preset_House_05:
                return "Tuscany";
            default:
                Debug.LogError($"Loca: GetThemeName: no theme name found for building name {buildingName}");
                return buildingName.ToString();
        }
    }

}