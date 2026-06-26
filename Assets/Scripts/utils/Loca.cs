using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class Loca
{
    public static string GetThemeName(BuildingName buildingName)
    {
        if (BuildingNameUtil.IsChallengeBuilding(buildingName))
        {
            var index = BuildingNameUtil.allBuildingNamesChallenges.FindIndex(g => g == buildingName);
            return "Challenge " + (index + 1);
        }
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
                return buildingName.ToString();
        }
    }

}