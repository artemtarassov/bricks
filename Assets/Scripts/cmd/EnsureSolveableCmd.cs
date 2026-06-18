using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class EnsureSolveableCmd
{
    public void Run()
    {
        //in case the data changed in the last update, the elements should be renewed to be sure the game is solveable. This is a safety measure to avoid any possible bugs with unsolveable levels.

        var pd = PlayerModel.Instance.playerData;
        var progress = pd.Progress;
        foreach (var p in progress)
        {
            if (p.State == BuildingState.Unlocked || p.State == BuildingState.Playing)
            {
                var currentElement = p.GetCurrentElement();
                if (currentElement == null)
                {
                    continue;
                }
                if (currentElement.ElementCompleted())
                {
                    continue;
                }
                if (!ColorsValid(currentElement))
                {
                    Debug.LogError($"EnsureSolveableCmd: Colors not valid for building {p.BuildingName}, element {currentElement.dataKey}, renewing element data");
                    p.SetCurrentElement(BalancingModel.Instance.GetDataCopy(p.BuildingName, currentElement.dataKey));
                }
                pd.isDirty = true;
            }
        }
        var result = this.EnsureCurrentElementExists();
        Debug.Log("EnsureSolveableCmd: EnsureCurrentElementExists result: " + result);
    }

    private bool EnsureCurrentElementExists()
    {
        var pd = PlayerModel.Instance.playerData;
        var progress = pd.GetCurrentBuildingProgress();
        if (progress.GetCurrentElement() == null)
        {
            return true;
        }
        var element = progress.GetCurrentElement();
        if (element == null)
        {
            return true;
        }
        var bd = BalancingModel.Instance.GetDataCopy(progress.BuildingName, element.dataKey);
        if (bd == null)
        {
            progress.RemoveCurrentElement();
            return false;
        }
        var building = CityModel.Instance.GetBuildingByName(progress.BuildingName);
        if (building == null)
        {
            progress.RemoveCurrentElement();
            pd.RemoveCurrentBuilding();
            return false;
        }
        var elements = building.GetElements();
        var elementExists = elements.ToList().Exists(e => e.dataKey == element.dataKey);
        if (!elementExists)
        {
            progress.RemoveCurrentElement();
            return false;
        }
        return true;

    }


    private bool ColorsValid(CityElementDataContainer p)
    {
        var slotColorsLeft = new Dictionary<ColorIndex, int>();
        var bricksInColumns = p.columns.SelectMany(c => c.list).Where(e => e.BrickData != null).Select(e => e.BrickData).ToList();
        foreach (var b in bricksInColumns)
        {
            var color = b.color;
            if (!slotColorsLeft.ContainsKey(color))
            {
                slotColorsLeft[color] = 0;
            }
            slotColorsLeft[color] += b.coloredAmount;
        }

        var elementColorsNeeded = new Dictionary<ColorIndex, int>();
        foreach (var brick in p.brickDataList)
        {
            if (!elementColorsNeeded.ContainsKey(brick.color))
            {
                elementColorsNeeded[brick.color] = 0;
            }
            elementColorsNeeded[brick.color] += brick.coloredAmount + brick.transparentAmount;
        }

        return CompareDicts(slotColorsLeft, elementColorsNeeded);
    }


    private static bool CompareDicts(Dictionary<ColorIndex, int> dict1, Dictionary<ColorIndex, int> dict2)
    {
        if (dict1.Count != dict2.Count)
        {
            return false;
        }
        foreach (var kvp in dict1)
        {
            var key = kvp.Key;
            var value = kvp.Value;
            if (!dict2.ContainsKey(key) || dict2[key] != value)
            {
                return false;
            }
        }
        foreach (var kvp in dict2)
        {
            var key = kvp.Key;
            var value = kvp.Value;
            if (!dict1.ContainsKey(key) || dict1[key] != value)
            {
                return false;
            }
        }

        return true;
    }
}