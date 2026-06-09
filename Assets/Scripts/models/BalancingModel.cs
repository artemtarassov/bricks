using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public static int StartGroupIndex = 1;
    public static int FirstCityElementIndex = 0;

    private GroupDataListContainer groups;

    public BalancingModel()
    {
    }


    public void Load()
    {
        var resource = Resources.Load<TextAsset>($"{BalancingWriter.fileName}");
        if (resource == null)
        {
            this.groups = new GroupDataListContainer();
            return;
        }
        var json = resource.text;
        if (json.Length < 10)
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

        foreach (var g in this.groups.groups)
        {
            Debug.Log("BalancingModel Group " + g.groupName + " has cityElement " + g.cityElementDataList.Count);
        }
    }

    public List<string> GetAllGroups()
    {
        Assert.IsNotNull(this.groups, "BalancingModel GetAllGroups: groups is null, did you forget to call Load()?");
        var list = new List<string>();
        foreach (var g in this.groups.groups)
        {
            list.Add(g.groupName);
        }
        return list;
    }

    public GroupDataList GetDataCopy(string groupName)
    {
        Assert.IsNotNull(this.groups, "BalancingModel GetData: groups is null, did you forget to call Load()?");
        Assert.IsTrue(this.groups.groups.Count > 0, "BalancingModel GetData: no groups available in groups list");
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

    public CityElementDataContainer GetDataCopy(string groupName, string dataKey)
    {
        Assert.IsNotNull(this.groups, "BalancingModel GetData: groups is null, did you forget to call Load()?");
        Assert.IsNotNull(groupName, "BalancingModel GetData: groupName is null");
        Assert.IsNotNull(dataKey, "BalancingModel GetData: dataKey is null");
        var group = this.groups.groups.Find(g => g.groupName == groupName);
        if (group == null)
        {
            Debug.LogError($"BalancingModel GetData: no data found for group {groupName}");
            return null;
        }
        var container = group.cityElementDataList.Find(c => c.dataKey == dataKey);
        if (container == null)
        {
            Debug.LogError($"BalancingModel GetData: no data found for dataKey {dataKey}");
            return null;
        }
        return container.Clone();
    }

    public int CountEntries()
    {
        Assert.IsNotNull(this.groups, "BalancingModel CountEntries: groups is null, did you forget to call Load()?");
        return this.groups.groups.Count;
    }
}


public class BalancingWriter
{
    public static readonly string fileName = "balancing/groups";

    public BalancingWriter()
    {
        this.groups = new GroupDataListContainer();
    }

    public CityElementDataContainer GetData(string groupName, string dataKey)
    {
        Assert.IsNotNull(this.groups, "BalancingModel GetData: groups is null, did you forget to call Load()?");
        Assert.IsNotNull(groupName, "BalancingModel GetData: groupName is null");
        Assert.IsNotNull(dataKey, "BalancingModel GetData: dataKey is null");
        var group = this.groups.groups.Find(g => g.groupName == groupName);
        if (group == null)        {
            Debug.LogError($"BalancingModel GetData: no data found for group {groupName}");
            return null;
        }
        var container = group.cityElementDataList.Find(c => c.dataKey == dataKey);
        if (container == null)        {
            Debug.LogError($"BalancingModel GetData: no data found for dataKey {dataKey}");
            return null;
        }
        return container; 
    }
    private GroupDataListContainer groups;
    private string GetFilePath()
    {
        return Application.dataPath + $"/Resources/{fileName}.json";
    }

    private string GetAssetPath()
    {
        return $"Assets/Resources/{fileName}.json";
    }

#if UNITY_EDITOR
    private void RefreshEditorResource()
    {
        var assetPath = GetAssetPath();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        if (System.IO.File.Exists(GetFilePath()))
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    }

    private void ReloadRuntimeModelIfNeeded()
    {
        if (Application.isPlaying && BalancingModel.Instance != null)
        {
            BalancingModel.Instance.Load();
            Debug.Log("BalancingModel: reloaded balancing data during play mode.");
        }
    }
#endif

    public List<string> GetAllGroups()
    {
        Assert.IsNotNull(this.groups, "BalancingWriter GetAllGroups: groups is null");
        var list = new List<string>();
        foreach (var g in this.groups.groups)
        {
            list.Add(g.groupName);
        }
        return list;
    }

    public void Delete()
    {
        var filePath = GetFilePath();
        //delete file if exists
        if (System.IO.File.Exists(filePath))
        {
            Debug.Log($"BalancingModel Delete: deleting file {filePath}");
            System.IO.File.Delete(filePath);
            var resource = Resources.Load<TextAsset>($"{fileName}");
            if (resource != null)
            {
                Resources.UnloadAsset(resource);
            }
#if UNITY_EDITOR
            RefreshEditorResource();
            ReloadRuntimeModelIfNeeded();
#endif
        }
    }
    public void Save()
    {
        var json = JsonUtility.ToJson(this.groups, true);

        var filePath = GetFilePath();
        //delete file if exists
        if (System.IO.File.Exists(filePath))
        {
            Debug.Log($"BalancingModel Save: deleting existing file {filePath}");
            var existingResource = Resources.Load<TextAsset>($"{fileName}");
            if (existingResource != null)
            {
                Resources.UnloadAsset(existingResource);
            }
            System.IO.File.Delete(filePath);
        }
        Debug.Log($"BalancingModel Save to " + filePath);
        System.IO.File.WriteAllText(filePath, json, Encoding.ASCII);
        var elements = this.groups.groups.Sum(g => g.cityElementDataList.Count);
        Debug.Log($"BalancingModel saved {json.Length} bytes to {filePath}, groups: {this.groups.groups.Count}, elements: {elements}");

#if UNITY_EDITOR
        RefreshEditorResource();
#endif
        Resources.Load<TextAsset>($"{fileName}");
#if UNITY_EDITOR
        ReloadRuntimeModelIfNeeded();
#endif
    }


    public void AddDataForElement(string groupName, string dataKey, List<BrickData> brickDataList, List<SlotColumnData> slotElementDataList)
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
            throw new Exception($"BalancingModel InsertData: data for group {groupName} and dataKey {dataKey} already exists, cannot insert duplicate data");
        }
        group.cityElementDataList.Add(new CityElementDataContainer() { dataKey = dataKey, brickDataList = brickDataList, columns = slotElementDataList });
    }

    public int CountEntries()
    {
        Assert.IsNotNull(this.groups, "BalancingModel CountEntries: groups is null, did you forget to call Load()?");
        return this.groups.groups.Count;
    }

}
