using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class FlyBrickData
{
    public FlyBrickData()
    {

    }
    public Vector3 from;
    public Transform targetBrick;
    public ColorIndex colorIndex;
}

public class CityModel
{
    public static CityModel Instance;

    public Action<FlyBrickData> OnFlyBrick;

    private List<CityElement> cityElements;
    private List<BuildingElement> buildings;

    public Action<CityElement> OnEnableDifferentColors;
    public Action<CityElement> OnElementCompleted;

    public Action<BuildingName> OnCurrentBuildingChanged;

    public BuildingElement GetBuildingByName(BuildingName buildingName)
    {
        Assert.IsNotNull(this.buildings, "CityModel GetGroupByName: groups list is null");
        Assert.IsTrue(this.buildings.Count > 0, "CityModel GetGroupByName: groups list is empty");
        Assert.IsFalse(buildingName == BuildingName.Undefined, "CityModel GetGroupByName: buildingName should not be undefined");
        return this.buildings.Find((g) => g.BuildingName == buildingName);
    }

    public void SetBuildings(List<BuildingElement> buildings, BuildingName currentBuildingName)
    {
        Assert.IsNotNull(buildings, "CityModel SetBuildings: groups list is null");
        Assert.IsTrue(buildings.Count > 0, "CityModel SetBuildings: groups list is empty");
        Assert.IsFalse(currentBuildingName == BuildingName.Undefined, "CityModel SetBuildings: currentBuildingName should not be undefined");
        Debug.Log($"CityModel SetBuildings: setting groups with current building name {currentBuildingName}");
        this.buildings = buildings;
        this.SetCurrentBuildingName(currentBuildingName);
        OnCurrentBuildingChanged?.Invoke(currentBuildingName);
    }

    public void SetCurrentBuildingName(BuildingName currentBuildingName)
    {
        Debug.Log($"CityModel SetCurrentBuildingName: setting current building name to {currentBuildingName}");
        var b = this.buildings.Find(g => g.BuildingName == currentBuildingName);
        Assert.IsNotNull(b, $"CityModel SetCurrentBuildingName: failed to find group with name {currentBuildingName}");
        this.cityElements = b.GetElements().ToList();
        foreach (var b1 in this.buildings)
        {
            b1.gameObject.SetActive(b1.BuildingName == currentBuildingName);
        }
    }

    public int GetBuildingNameIndex(BuildingName buildingName)
    {
        Assert.IsNotNull(this.buildings, "CityModel GetBuildingNameIndex: groups list is null");
        Assert.IsTrue(this.buildings.Count > 0, "CityModel GetBuildingNameIndex: groups list is empty");
        Assert.IsFalse(buildingName == BuildingName.Undefined, "CityModel GetBuildingNameIndex: buildingName should not be undefined");
        var index = this.buildings.FindIndex(g => g.BuildingName == buildingName);
        return index;
    }


    public void FlyBrick(Vector3 from, Transform targetBrick, ColorIndex colorIndex = ColorIndex.Undefined)
    {
        OnFlyBrick?.Invoke(new FlyBrickData { from = from, targetBrick = targetBrick, colorIndex = colorIndex });
    }


    public void DeactivateAllElements()
    {
        Debug.Log("CityModel DeactivateAllElements: deactivating all city elements");
        Assert.IsNotNull(cityElements, "CityModel DeactivateAllElements: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel DeactivateAllElements: cityElements list is empty");
        foreach (var ce in cityElements)
        {
            ce.SetActive(false);
        }
    }

    public void ActivateElements(int toIndex)
    {
        Debug.Log($"CityModel ActivateElements: activating elements up to index {toIndex}");
        Assert.IsNotNull(cityElements, "CityModel ActivateElements: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel ActivateElements: cityElements list is empty");
        Assert.IsTrue(toIndex < cityElements.Count, $"CityModel ActivateElements: toIndex {toIndex} is out of range");
        for (var i = 0; i <= toIndex && i < cityElements.Count; i++)
        {
            cityElements[i].SetActive(true);
            cityElements[i].EnableVisuals(true);
            cityElements[i].EnableBricks(false);
        }
        for (var i = toIndex + 1; i < cityElements.Count; i++)
        {
            cityElements[i].SetActive(false);
        }

    }

    public int GetElementIndex(CityElement element)
    {
        Assert.IsNotNull(cityElements, "CityModel GetElementIndex: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel GetElementIndex: cityElements list is empty");
        Assert.IsNotNull(element, "CityModel GetElementIndex: element is null");
        var index = cityElements.FindIndex(e => e == element);
        if (index < 0)
        {
            Debug.LogError($"CityModel GetElementIndex: failed to find index of city element with dataKey {element.dataKey}");
        }
        return index;
    }

    public CityElement GetElementByDataKey(string dataKey)
    {
        Assert.IsFalse(string.IsNullOrEmpty(dataKey), "CityModel GetElementByDataKey: dataKey should not be null or empty");
        Assert.IsNotNull(cityElements, "CityModel GetElementByDataKey: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel GetElementByDataKey: cityElements list is empty");
        var ce = cityElements.Find(e => e.dataKey == dataKey);
        if (ce == null)
        {
            Debug.LogError($"CityModel GetElementByDataKey: failed to find city element with dataKey {dataKey}");
            return null;
        }
        return ce;
    }


    public void EnableDifferentColors(CityElement cityElement, int n)
    {
        Assert.IsTrue(n > 0, "CityModel EnableDifferentColors: n should be greater than 0");
        Assert.IsNotNull(cityElement, "CityModel EnableDifferentColors: cityElement should not be null");
        var elementDataContainer = cityElement.dataContainer;
        Assert.IsNotNull(elementDataContainer, $"CityModel EnableDifferentColors: dataContainer should not be null for city element {cityElement.name}");
        elementDataContainer.EnableDifferentColors(n);
        cityElement.ShowCurrentState();
        OnEnableDifferentColors?.Invoke(cityElement);
    }

    public CityElement GetElementByIndex(int index)
    {
        Assert.IsNotNull(cityElements, "CityModel GetElementByIndex: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel GetElementByIndex: cityElements list is empty");
        Assert.IsTrue(index >= 0 && index < cityElements.Count, $"CityModel GetElementByIndex: index {index} is out of range");
        return cityElements[index];
    }
}