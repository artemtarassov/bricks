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
            BalancingModel.Instance = new BalancingModel();
            BalancingModel.Instance.Load();
            var selectedObjects = Selection.gameObjects;
            foreach (var obj in selectedObjects)
            {
                var cityElement = obj.GetComponent<CityElement>();
                if (cityElement != null)
                {
                    OnAddBalancingDataClicked(cityElement);
                }
            }
        }
    }


    public void ApplyDifficulty(string key, int difficulty = 1)//0-3
    {
        Assert.IsTrue(difficulty >= 0 && difficulty <= 3, "ApplyDifficulty: difficulty should be between 0 and 3");
        var counter = 0;
        var data = BalancingModel.Instance.GetDataCopy(key);
        var columnIndexList = new List<int>();
        for (var i = 0; i < SlotModel.MaxColumns; i++)
        {
            columnIndexList.Add(i);
        }
        var shuffledColumnIndexes = new NonRepeatingShuffleBag<int>(columnIndexList);
        var lastCommonRowIndex = data.slotElementDataList.Min(s => s.list.Count - 1);
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
                var randColumn = data.slotElementDataList.Find(s => s.columnIndex == randColumnIndex);

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

        BalancingModel.Instance.Save();
        Debug.Log($"BalancingWindow ApplyDifficulty: total swaps applied: {counter}, difficulty: {difficulty}   ");
    }

    private void OnAddBalancingDataClicked(CityElement cityElement)
    {

        var key = cityElement.dataKey;
        var cec = new CityElementColors(cityElement.FindAllBricks().Count);
        var predefinedBricks = cec.predefinedBricks.ToList();
        Assert.IsTrue(predefinedBricks.Count > 0, "predefinedBricks is empty");

        var slotElementDataList = new List<SlotElementDataList>();

        var maxColumns = SlotModel.MaxColumns;

        while (predefinedBricks.Count > 0)
        {
            for (var c = 0; c < maxColumns && predefinedBricks.Count > 0; c++)
            {
                var b = predefinedBricks.First().Clone();
                predefinedBricks.RemoveAt(0);
                if (slotElementDataList.Count <= c)
                {
                    slotElementDataList.Add(new SlotElementDataList() { columnIndex = c });
                }
                slotElementDataList[c].list.Add(new SlotElementData(b));
            }
        }

        BalancingModel.Instance.InsertData(key, cec.predefinedBricks, slotElementDataList);
        var entries = BalancingModel.Instance.CountEntries();
        Debug.Log($"BalancingWindow: total entries in BalancingModel: {entries}");


        var cityElementIndex = cityElement.transform.GetSiblingIndex();//0-n
        Debug.Log($"BalancingWindow: city element index: {cityElementIndex}");
        //0= difficulty easy
        //1= difficulty medium
        //2= difficulty hard
        //3= difficulty easy
        //4= difficulty medium
        //5= difficulty hard
        //etc
        var difficulty = (cityElementIndex / 2) % 3 + 1;
        if (cityElementIndex == 0)
        {
            difficulty = 0;
        }
        this.ApplyDifficulty(key, difficulty);
    }

}
