using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
class DataContainerList
{
    public List<CityElementDataContainer> list;
}

[Serializable]
class CityElementDataContainer
{
    public string key;
    public List<BrickData> brickDataList;
    public List<SlotElementDataList> slotElementDataList;
}

public class BalancingModel
{
    public static readonly BalancingModel Instance = new BalancingModel();


    public void Load()
    {
        var resource = Resources.Load<TextAsset>("balancing/bricks_and_columns");
        if (resource == null)
        {
            Debug.LogError("BalancingModel Load: failed to load bricks_and_columns.json from resources");
            return;
        }
        var json = resource.text;
        Debug.Log($"BalancingModel Load: loaded json from resources, json: {json}");
        var data = JsonUtility.FromJson<DataContainerList>(json);
        Assert.IsNotNull(data, "BalancingModel Load: failed to parse bricks_and_columns.json");
        Assert.IsNotNull(data.list, "BalancingModel Load: data.list is null after parsing bricks_and_columns.json");
        Debug.Log($"BalancingModel Load: loaded {data.list.Count} entries from bricks_and_columns.json");
    }
}
