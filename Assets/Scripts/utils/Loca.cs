using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class Loca
{
    public static string GetThemeName(string groupName)
    {
        switch (groupName)
        {
            case "Ruins1_House":
                return "Palermo";
            case "Tower_House":
                return "Veneto";
            case "Preset_House_05":
                return "Tuscany";
            default:
                Debug.LogError($"Loca: GetThemeName: no theme name found for group name {groupName}");
                return groupName;
        }
    }

}