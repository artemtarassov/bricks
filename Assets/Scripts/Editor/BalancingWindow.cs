using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class BalancingWindow : EditorWindow
{
    [MenuItem("Tools/Balancing Window")]
    public static void ShowWindow()
    {
        GetWindow<BalancingWindow>("Balancing");
    }

    private void OnGUI()
    {

        if (GUILayout.Button("Add balancing data for city element"))
        {
            new PrepareScene().Run();
            var model = new BalancingWriter();

            var buildings1 = GameObject.FindObjectsByType<BuildingElement>(FindObjectsInactive.Include).ToList();
            foreach (var b in buildings1)
            {
                var elements = b.GetElements();
                Debug.Log($"BalancingWindow: found group {b.BuildingName} with {elements.Count()} elements in selected objects, adding balancing data for each element.");
                foreach (var cityElement in elements)
                {
                    OnAddBalancingDataClicked(model, b, cityElement);
                }
            }
            model.Save();
        }

    }


    public void ApplyDifficulty(BalancingWriter model, BuildingName buildingName, string dataKey, int difficulty = 1)//0-3
    {
        Assert.IsTrue(difficulty >= 0 && difficulty <= 3, "ApplyDifficulty: difficulty should be between 0 and 3");
        var counter = 0;
        var data = model.GetData(buildingName, dataKey);
        var columnIndexList = new List<int>();
        for (var i = 0; i < SlotModel.MaxColumns; i++)
        {
            columnIndexList.Add(i);
        }
        var shuffledColumnIndexes = new NonRepeatingShuffleBag<int>(columnIndexList);
        var lastCommonRowIndex = data.columns.Min(s => s.list.Count - 1);
        var startRowIndex = 1;

        if (difficulty == 0)
        {
            startRowIndex = 2;
            difficulty = 1;
        }

        for (var i = startRowIndex; i <= lastCommonRowIndex; i += 2)
        {
            for (var d = 0; d < difficulty; d++)
            {
                var randColumnIndex = shuffledColumnIndexes.GetNext();
                var randColumn = data.columns.Find(s => s.columnIndex == randColumnIndex);

                var from = randColumn.list[i];
                var to = randColumn.list[i - 1];

                if (from.IsBrick)
                {
                    var isSame = from.BrickData.color == to.BrickData.color;
                    if (!isSame)
                    {
                        ListUtil.Swap(randColumn.list, i, i - 1);
                        counter++;
                    }
                    else
                    {
                        i--;
                        break;
                    }
                }
                else
                {
                    i--;
                    break;
                }

            }
        }

        //Debug.Log($"BalancingWindow ApplyDifficulty: total swaps applied: {counter}, difficulty: {difficulty}   ");
    }

    private void OnAddBalancingDataClicked(BalancingWriter model, BuildingElement b, CityElement cityElement)
    {

        var dataKey = cityElement.dataKey;
        var cec = new CityElementColors(cityElement.FindAllBricks().Count);
        var predefinedBricks = cec.predefinedBricks.ToList();
        Assert.IsTrue(predefinedBricks.Count > 0, "predefinedBricks is empty");

        var slotElementDataList = new List<SlotColumnData>();

        var maxColumns = SlotModel.MaxColumns;

        while (predefinedBricks.Count > 0)
        {
            for (var c = 0; c < maxColumns && predefinedBricks.Count > 0; c++)
            {
                var br = predefinedBricks.First().Clone();
                br.SetAll(BrickState.Colored);
                predefinedBricks.RemoveAt(0);
                if (slotElementDataList.Count <= c)
                {
                    slotElementDataList.Add(new SlotColumnData() { columnIndex = c });
                }
                slotElementDataList[c].list.Add(new SlotElementData(br));
            }
        }
        var groupName = b.BuildingName;
        model.AddDataForElement(groupName, dataKey, cec.predefinedBricks, slotElementDataList);


        var cityElementIndex = cityElement.transform.GetSiblingIndex();//0-n
        //Debug.Log($"BalancingWindow: city element index: {cityElementIndex}");
        //0= difficulty easy
        //1= difficulty medium
        //2= difficulty hard
        //3= difficulty easy
        //4= difficulty medium
        //5= difficulty hard
        //etc
        /*var difficulty = (cityElementIndex / 2) % 3 + 1;
        if (cityElementIndex == 0)
        {
            difficulty = 0;
        }*/
        var difficulty = cityElementIndex % 2 == 0 ? 0 : 1;
        this.ApplyDifficulty(model, groupName, dataKey, difficulty);

        Debug.Log($"BalancingWindow: added balancing data for element {dataKey} in group {groupName} with difficulty {difficulty}");

    }

}
