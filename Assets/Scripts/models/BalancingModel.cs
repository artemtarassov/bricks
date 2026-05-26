using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private readonly string fileName = "groups";


    private GroupDataListContainer groups;
    private GroupDataList GetDataContainerList(string groupName)
    {
        Assert.IsNotNull(this.groups, "BalancingModel GetDataContainerList: groups is null, did you forget to call Load()?");
        Assert.IsNotNull(groupName, "BalancingModel GetDataContainerList: groupName is null");
        var list = this.groups.groups.Find(g => g.groupName == groupName);
        if (list == null)
        {
            Debug.LogError($"BalancingModel GetDataContainerList: no data found for group {groupName}");
            return null;
        }
        return list;
    }

    public BalancingModel()
    {
    }


    public void Load()
    {
        var resource = Resources.Load<TextAsset>($"balancing/{this.fileName}");
        if (resource == null)
        {
            this.groups = new GroupDataListContainer();
            return;
        }
        var json = resource.text;
        if (json.Length < 1)
        {
            this.groups = new GroupDataListContainer();
            return;
        }
        Debug.Log($"BalancingModel Load: loaded json from resources, json: {json}");
        var data = JsonUtility.FromJson<GroupDataListContainer>(json);
        Assert.IsNotNull(data, "BalancingModel Load: failed to parse groups.json");
        Assert.IsNotNull(data.groups, "BalancingModel Load: data.list is null after parsing groups.json");
        Debug.Log($"BalancingModel Load: loaded {data.groups.Count} entries from groups.json");
        this.groups = data;
    }

    public GroupDataList GetDataCopy(string groupName)
    {
        Assert.IsNotNull(this.groups, "BalancingModel GetData: groups is null, did you forget to call Load()?");
        Assert.IsNotNull(groupName, "BalancingModel GetData: groupName is null");
        var group = this.groups.groups.Find(g => g.groupName == groupName);
        if (group == null)
        {
            Debug.LogError($"BalancingModel GetData: no data found for group {groupName}");
            return null;
        }
        return group.Clone();
    }

    public string GetNextGroup(string groupName)
    {
        Assert.IsNotNull(this.groups, "BalancingModel GetNextGroup: groups is null, did you forget to call Load()?");
        Assert.IsNotNull(groupName, "BalancingModel GetNextGroup: groupName is null");
        var index = this.groups.groups.FindIndex(g => g.groupName == groupName);
        if (index < 0)
        {
            Debug.LogError($"BalancingModel GetNextGroup: no data found for group {groupName}");
            return null;
        }
        var nextIndex = index + 1;
        if (nextIndex >= this.groups.groups.Count)
        {
            throw new Exception($"BalancingModel GetNextGroup: no next group found for group {groupName}, index: {index}, nextIndex: {nextIndex}, total groups: {this.groups.groups.Count}");
        }
        return this.groups.groups[nextIndex].groupName;
    }

    public CityElementDataContainer GetDataCopy(string groupName, string key)
    {
        Assert.IsNotNull(this.groups, "BalancingModel GetData: groups is null, did you forget to call Load()?");
        Assert.IsNotNull(groupName, "BalancingModel GetData: groupName is null");
        Assert.IsNotNull(key, "BalancingModel GetData: key is null");
        var group = this.groups.groups.Find(g => g.groupName == groupName);
        if (group == null)
        {
            Debug.LogError($"BalancingModel GetData: no data found for group {groupName}");
            return null;
        }
        var container = group.cityElementDataList.Find(c => c.dataKey == key);
        if (container == null)
        {
            Debug.LogError($"BalancingModel GetData: no data found for key {key}");
            return null;
        }
        return container.Clone();
    }

#if UNITY_EDITOR
    public void Save()
    {
        var json = JsonUtility.ToJson(this.groups, false);
        Debug.Log($"BalancingModel InsertData: saving data to json, json: {json}");
        System.IO.File.WriteAllText(Application.dataPath + $"/Resources/balancing/{this.fileName}.json", json);
    }
#endif

    public void InsertData(string groupName, string dataKey, List<BrickData> brickDataList, List<SlotElementDataList> slotElementDataList)
    {
        Assert.IsNotNull(this.groups, "BalancingModel InsertData: groups is null, did you forget to call Load()?");
        Assert.IsNotNull(groupName, "BalancingModel InsertData: groupName is null");
        Assert.IsNotNull(dataKey, "BalancingModel InsertData: dataKey is null");
        Assert.IsNotNull(brickDataList, "BalancingModel InsertData: brickDataList is null");
        Assert.IsNotNull(slotElementDataList, "BalancingModel InsertData: slotElementDataList is null");
        Assert.IsTrue(brickDataList.Count > 0, "BalancingModel InsertData: brickDataList is empty");
        Assert.IsTrue(slotElementDataList.Count > 0, "BalancingModel InsertData: slotElementDataList is empty");

        //remove prev if exists
        var group = this.groups.groups.Find(g => g.groupName == groupName);
        if (group == null)
        {
            group = new GroupDataList(groupName);
            this.groups.groups.Add(group);
        }
        var prev = group.cityElementDataList.Find(c => c.dataKey == dataKey);
        if (prev != null)
        {
            group.cityElementDataList.Remove(prev);
        }
        group.cityElementDataList.Add(new CityElementDataContainer() { dataKey = dataKey, brickDataList = brickDataList, slotElementDataList = slotElementDataList });
        this.Save();
    }

    public int CountEntries()
    {
        Assert.IsNotNull(this.groups, "BalancingModel CountEntries: groups is null, did you forget to call Load()?");
        return this.groups.groups.Count;
    }
}
