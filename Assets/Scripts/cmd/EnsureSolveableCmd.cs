using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnsureSolveableCmd
{
    public void Run()
    {
        //in case the data changed in the last update, the elements should be renewed to be sure the game is solveable. This is a safety measure to avoid any possible bugs with unsolveable levels.

        var pd = PlayerModel.Instance.playerData;
        var progress = pd.progress;
        foreach (var p in progress)
        {
            if (p.state == GroupState.Unlocked || p.state == GroupState.Playing)
            {
                if (p.currentElement == null)
                {
                    continue;
                }
                if (p.currentElement.ElementCompleted() || ColorsValid(p.currentElement))
                {
                    continue;
                }
                var balancing = BalancingModel.Instance.GetDataCopy(p.groupName, p.currentElement.dataKey);
                p.currentElement.columns = balancing.columns;
                p.currentElement.brickDataList = balancing.brickDataList;
                pd.isDirty = true;
            }
        }
    }


    private bool ColorsValid(CityElementDataContainer p)
    {
        var slotColorsLeft = new Dictionary<ColorIndex, int>();
        foreach (var column in p.columns)
        {
            foreach (var slotElementData in column.list)
            {
                if (slotElementData.type == SlotElementType.Bricks || slotElementData.type == SlotElementType.HiddenBricks)
                {
                    var color = slotElementData.brickData.color;
                    if (!slotColorsLeft.ContainsKey(color))
                    {
                        slotColorsLeft[color] = 0;
                    }
                    slotColorsLeft[color]++;
                }
            }
        }

        var elementColorsNeeded = new Dictionary<ColorIndex, int>();
        foreach (var brick in p.brickDataList)
        {
            if (!elementColorsNeeded.ContainsKey(brick.color))
            {
                elementColorsNeeded[brick.color] = 0;
            }
            elementColorsNeeded[brick.color] += brick.coloredAmount;
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