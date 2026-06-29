using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class Loca
{
    public static string GetBuildingNameTranslation(BuildingName buildingName)
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
            case BuildingName.Challenge_House_Cat01:
                return "Cat";
            case BuildingName.Challenge_House_Bear01:
                return "Bear";
            case BuildingName.Challenge_House_Cake01:
                return "Cake";
            case BuildingName.Challenge_House_Crystals01:
                return "Crystals";
            case BuildingName.Challenge_House_Frog01:
                return "Frog";
            case BuildingName.Challenge_House_HotChocolate01:
                return "Hot Chocolate";
            case BuildingName.Challenge_House_Octopus01:
                return "Octopus";
            case BuildingName.Challenge_House_Rabbit01:
                return "Rabbit";
            case BuildingName.Challenge_House_Sailbot01:
                return "Sailbot";
            case BuildingName.Challenge_House_Shark01:
                return "Shark";
            default:
                return buildingName.ToString();
        }
    }

}