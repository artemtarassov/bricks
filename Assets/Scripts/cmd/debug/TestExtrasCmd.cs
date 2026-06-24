using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TestExtrasCmd
{
    public void Run()
    {
        var buildingDataList = BalancingModel.Instance.GetDataCopy(BuildingName.Preset_Bath_House_01);
        var elementList = buildingDataList.cityElementDataList;
        elementList.Sort((a, b) => a.brickDataList.Count.CompareTo(b.brickDataList.Count));
        elementList.Reverse();

        var firstElement = elementList[0];
        var bricks = firstElement.brickDataList.Count;

        new AddExtrasCmd(BuildingName.Preset_Bath_House_01).Run(firstElement, 4);

        var columns = firstElement.columns;
        var buff = "";
        for (var row = 0; row < 100; row++)
        {

            foreach (var c in columns)
            {
                if (row >= c.list.Count)
                {
                    buff += " [___]";
                }
                else
                {
                    if (c.list[row].type == SlotElementType.Bricks)
                    {
                        buff += " [XXX]";
                    }
                    else
                        buff += " [" + c.list[row].type.ToString().Substring(0, 3) + "]";
                }
            }
            buff += "\n";
        }
        Debug.Log($"TestExtrasCmd (bricks: {bricks}): Run: firstElement:\n{buff}");


    }

}