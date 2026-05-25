using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BalancingModel
{
    public static BalancingModel Instance;

    public static NonRepeatingShuffleBag<int> shuffledAmounts = new NonRepeatingShuffleBag<int>(new List<int>()
        { 5,5,5,10,10,15}
);
    public static NonRepeatingShuffleBag<ColorIndex> shuffledColorIndexes = new NonRepeatingShuffleBag<ColorIndex>(new List<ColorIndex>()
        { ColorIndex.C0, ColorIndex.C1, ColorIndex.C2, ColorIndex.C3, ColorIndex.C4, ColorIndex.C5, ColorIndex.C6 }
    );

    public static int AdditionalBricksOnEmptyElement = 3;


    private DataContainerList dataContainerList;
    public BalancingModel()
    {
    }

    public bool DidLoad()
    {
        return this.dataContainerList != null;
    }
    public void Load()
    {
        var resource = Resources.Load<TextAsset>("balancing/bricks_and_columns");
        if (resource == null)
        {
            this.dataContainerList = new DataContainerList() { list = new List<CityElementDataContainer>() };
            return;
        }
        var json = resource.text;
        if (json.Length < 1)
        {
            this.dataContainerList = new DataContainerList() { list = new List<CityElementDataContainer>() };
            return;
        }
        Debug.Log($"BalancingModel Load: loaded json from resources, json: {json}");
        var data = JsonUtility.FromJson<DataContainerList>(json);
        Assert.IsNotNull(data, "BalancingModel Load: failed to parse bricks_and_columns.json");
        Assert.IsNotNull(data.list, "BalancingModel Load: data.list is null after parsing bricks_and_columns.json");
        Debug.Log($"BalancingModel Load: loaded {data.list.Count} entries from bricks_and_columns.json");
        this.dataContainerList = data;
    }

    public CityElementDataContainer GetDataCopy(string key)
    {
        Assert.IsNotNull(this.dataContainerList, "BalancingModel GetData: dataContainerList is null, did you forget to call Load()?");
        Assert.IsNotNull(key, "BalancingModel GetData: key is null");
        var container = this.dataContainerList.list.Find(c => c.key == key);
        if (container == null)
        {
            Debug.LogError($"BalancingModel GetData: no data found for key {key}");
            return null;
        }
        return container.Clone();
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(this.dataContainerList, true);
        Debug.Log($"BalancingModel InsertData: saving data to json, json: {json}");
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/balancing/bricks_and_columns.json", json);
    }

    public void InsertData(string key, List<BrickData> brickDataList, List<SlotElementDataList> slotElementDataList)
    {
        Assert.IsNotNull(this.dataContainerList, "BalancingModel InsertData: dataContainerList is null, did you forget to call Load()?");
        Assert.IsNotNull(key, "BalancingModel InsertData: key is null");
        Assert.IsNotNull(brickDataList, "BalancingModel InsertData: brickDataList is null");
        Assert.IsNotNull(slotElementDataList, "BalancingModel InsertData: slotElementDataList is null");
        Assert.IsTrue(brickDataList.Count > 0, "BalancingModel InsertData: brickDataList is empty");
        Assert.IsTrue(slotElementDataList.Count > 0, "BalancingModel InsertData: slotElementDataList is empty");

        //remove prev if exists
        var prev = this.dataContainerList.list.Find(c => c.key == key);
        if (prev != null)
        {
            this.dataContainerList.list.Remove(prev);
        }
        this.dataContainerList.list.Add(new CityElementDataContainer() { key = key, brickDataList = brickDataList, slotElementDataList = slotElementDataList });
        this.Save();
    }

    public int CountEntries()
    {
        Assert.IsNotNull(this.dataContainerList, "BalancingModel CountEntries: dataContainerList is null, did you forget to call Load()?");
        return this.dataContainerList.list.Count;
    }
}
