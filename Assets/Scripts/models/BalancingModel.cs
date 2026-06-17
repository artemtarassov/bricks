using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public static int FirstCityElementIndex = 0;

    private BuildingDataContainer buildingDataContainer;

    public BalancingModel()
    {
    }


    public void Load()
    {
        var resource = Resources.Load<TextAsset>($"{BalancingWriter.fileName}");
        if (resource == null)
        {
            this.buildingDataContainer = new BuildingDataContainer();
            return;
        }
        var json = resource.text;
        if (json.Length < 10)
        {
            this.buildingDataContainer = new BuildingDataContainer();
            return;
        }
        //Debug.Log($"BalancingModel Load: loaded json from resources, json: {json}");
        var data = JsonUtility.FromJson<BuildingDataContainer>(json);
        Assert.IsNotNull(data, "BalancingModel Load: failed to parse groups.json");
        Assert.IsNotNull(data.buildings, "BalancingModel Load: data.list is null after parsing groups.json");
        Debug.Log($"BalancingModel Load: loaded {data.buildings.Count} entries from groups.json");
        this.buildingDataContainer = data;

        foreach (var g in this.buildingDataContainer.buildings)
        {
            Debug.Log("BalancingModel Group " + g.BuildingName + " has cityElement " + g.cityElementDataList.Count);
        }
    }

    public BuildingData GetDataCopy(BuildingName buildingName)
    {
        Assert.IsNotNull(this.buildingDataContainer, "BalancingModel GetData: groups is null, did you forget to call Load()?");
        Assert.IsTrue(this.buildingDataContainer.buildings.Count > 0, "BalancingModel GetData: no groups available in groups list");
        Assert.IsFalse(buildingName == BuildingName.Undefined, "BalancingModel GetData: buildingName is undefined");
        var d = this.buildingDataContainer.buildings.Find(g => g.BuildingName == buildingName);
        Assert.IsNotNull(d, $"BalancingModel GetData: no data found for building {buildingName}");
        return d.Clone();
    }

    public BuildingName GetNextGroup(BuildingName buildingName)
    {
        Assert.IsNotNull(this.buildingDataContainer, "BalancingModel GetNextGroup: groups is null, did you forget to call Load()?");
        Assert.IsFalse(buildingName == BuildingName.Undefined, "BalancingModel GetNextGroup: buildingName is undefined");
        var index = this.buildingDataContainer.buildings.FindIndex(g => g.BuildingName == buildingName);
        Assert.IsTrue(index >= 0, $"BalancingModel GetNextGroup: no data found for building {buildingName}");
        var nextIndex = index + 1;
        if (nextIndex >= this.buildingDataContainer.buildings.Count)
        {
            throw new Exception($"BalancingModel GetNextGroup: no next building found for building {buildingName}, index: {index}, nextIndex: {nextIndex}, total buildings: {this.buildingDataContainer.buildings.Count}");
        }
        return this.buildingDataContainer.buildings[nextIndex].BuildingName;
    }

    public CityElementDataContainer GetDataCopy(BuildingName buildingName, string dataKey)
    {
        Assert.IsNotNull(this.buildingDataContainer, "BalancingModel GetData: groups is null, did you forget to call Load()?");
        Assert.IsFalse(buildingName == BuildingName.Undefined, "BalancingModel GetData: buildingName is undefined");
        Assert.IsFalse(string.IsNullOrEmpty(dataKey), "BalancingModel GetData: dataKey is null or empty");
        var group = this.buildingDataContainer.buildings.Find(g => g.BuildingName == buildingName);
        if (group == null)
        {
            Debug.LogError($"BalancingModel GetData: no data found for building {buildingName}");
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
        Assert.IsNotNull(this.buildingDataContainer, "BalancingModel CountEntries: groups is null, did you forget to call Load()?");
        return this.buildingDataContainer.buildings.Count;
    }
}


public class BalancingWriter
{
    public static readonly string fileName = "balancing/groups";

    public BalancingWriter()
    {
        this.buildingDataContainer = new BuildingDataContainer();
    }

    public CityElementDataContainer GetData(BuildingName buildingName, string dataKey)
    {
        Assert.IsNotNull(this.buildingDataContainer, "BalancingModel GetData: groups is null, did you forget to call Load()?");
        Assert.IsTrue(this.buildingDataContainer.buildings.Count > 0, "BalancingModel GetData: no groups available in groups list");
        Assert.IsFalse(buildingName == BuildingName.Undefined, "BalancingModel GetData: buildingName is undefined");
        Assert.IsFalse(string.IsNullOrEmpty(dataKey), "BalancingModel GetData: dataKey is null or empty");
        var group = this.buildingDataContainer.buildings.Find(g => g.BuildingName == buildingName);
        if (group == null)        {
            Debug.LogError($"BalancingModel GetData: no data found for building {buildingName}");
            return null;
        }
        var container = group.cityElementDataList.Find(c => c.dataKey == dataKey);
        if (container == null)        {
            Debug.LogError($"BalancingModel GetData: no data found for dataKey {dataKey}");
            return null;
        }
        return container; 
    }
    private BuildingDataContainer buildingDataContainer;
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

    public List<BuildingName> GetAllBuildingNames()
    {
        return BuildingNameUtil.GetAllBuildingNames();
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
        var json = JsonUtility.ToJson(this.buildingDataContainer, true);

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
        var elements = this.buildingDataContainer.buildings.Sum(g => g.cityElementDataList.Count);
        Debug.Log($"BalancingModel saved {json.Length} bytes to {filePath}, groups: {this.buildingDataContainer.buildings.Count}, elements: {elements}");

#if UNITY_EDITOR
        RefreshEditorResource();
#endif
        Resources.Load<TextAsset>($"{fileName}");
#if UNITY_EDITOR
        ReloadRuntimeModelIfNeeded();
#endif
    }


    public void AddDataForElement(BuildingName buildingName, string dataKey, List<BrickData> brickDataList, List<SlotColumnData> slotElementDataList)
    {
        Assert.IsNotNull(this.buildingDataContainer, "BalancingModel InsertData: buildingDataContainer is null, did you forget to call Load()?");
        Assert.IsFalse(buildingName == BuildingName.Undefined, "BalancingModel InsertData: buildingName is undefined");
        Assert.IsFalse(string.IsNullOrEmpty(dataKey), "BalancingModel InsertData: dataKey is null or empty");
        Assert.IsNotNull(brickDataList, "BalancingModel InsertData: brickDataList is null");
        Assert.IsNotNull(slotElementDataList, "BalancingModel InsertData: slotElementDataList is null");
        Assert.IsTrue(brickDataList.Count > 0, "BalancingModel InsertData: brickDataList is empty");
        Assert.IsTrue(slotElementDataList.Count > 0, "BalancingModel InsertData: slotElementDataList is empty");

        //remove prev if exists
        var building = this.buildingDataContainer.buildings.Find(g => g.BuildingName == buildingName);
        if (building == null)
        {
            building = new BuildingData(buildingName);
            this.buildingDataContainer.buildings.Add(building);
        }
        var prev = building.cityElementDataList.Find(c => c.dataKey == dataKey);
        if (prev != null)
        {
            throw new Exception($"BalancingModel InsertData: data for building {buildingName} and dataKey {dataKey} already exists, cannot insert duplicate data");
        }
        building.cityElementDataList.Add(new CityElementDataContainer(dataKey) { brickDataList = brickDataList, columns = slotElementDataList });
    }

    public int CountEntries()
    {
        Assert.IsNotNull(this.buildingDataContainer, "BalancingModel CountEntries: buildingDataContainer is null, did you forget to call Load()?");
        return this.buildingDataContainer.buildings.Count;
    }

}
