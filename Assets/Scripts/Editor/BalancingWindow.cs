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
        GameObject activeSelectedObject = Selection.activeGameObject;
        bool canAddBalancingData = activeSelectedObject != null && activeSelectedObject.GetComponent<CityElementGroup>() != null;

        using (new EditorGUI.DisabledScope(!canAddBalancingData))
        {
            if (GUILayout.Button("Add balancing data for city element"))
            {
                var model = new BalancingWriter();

                var ngroups = model.GetAllGroups();
                Assert.IsTrue(ngroups.Count == 0, "BalancingWindow: expected no groups in model after delete, but found " + ngroups.Count);


                var selectedObjects = Selection.gameObjects;
                foreach (var obj in selectedObjects)
                {
                    var group = obj.GetComponent<CityElementGroup>();
                    if (group != null)
                    {
                        var elements = group.GetElements();
                        Debug.Log($"BalancingWindow: found group {group.GroupName} with {elements.Count()} elements in selected objects, adding balancing data for each element.");
                        foreach (var cityElement in elements)
                        {
                            OnAddBalancingDataClicked(model, group, cityElement);
                        }
                    }
                }
                model.Save();
            }
        }
    }


    public void ApplyDifficulty(BalancingWriter model, string groupName, string dataKey, int difficulty = 1)//0-3
    {
        Assert.IsTrue(difficulty >= 0 && difficulty <= 3, "ApplyDifficulty: difficulty should be between 0 and 3");
        var counter = 0;
        var data = model.GetData(groupName, dataKey);
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

                if (from.type == SlotElementType.Bricks && to.type == SlotElementType.Bricks)
                {
                    var isSame = from.brickData.color == to.brickData.color;
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

    private void OnAddBalancingDataClicked(BalancingWriter model, CityElementGroup group, CityElement cityElement)
    {

        var data = cityElement.dataKey;
        var cec = new CityElementColors(cityElement.FindAllBricks().Count);
        var predefinedBricks = cec.predefinedBricks.ToList();
        Assert.IsTrue(predefinedBricks.Count > 0, "predefinedBricks is empty");

        var slotElementDataList = new List<SlotColumnData>();

        var maxColumns = SlotModel.MaxColumns;

        while (predefinedBricks.Count > 0)
        {
            for (var c = 0; c < maxColumns && predefinedBricks.Count > 0; c++)
            {
                var b = predefinedBricks.First().Clone();
                b.SetAll(BrickState.Colored);
                predefinedBricks.RemoveAt(0);
                if (slotElementDataList.Count <= c)
                {
                    slotElementDataList.Add(new SlotColumnData() { columnIndex = c });
                }
                slotElementDataList[c].list.Add(new SlotElementData(b));
            }
        }
        var groupName = group.GroupName;
        model.AddDataForElement(groupName, data, cec.predefinedBricks, slotElementDataList);


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
        this.ApplyDifficulty(model, groupName, data, difficulty);

        Debug.Log($"BalancingWindow: added balancing data for element {data} in group {groupName} with difficulty {difficulty}");

    }

}
