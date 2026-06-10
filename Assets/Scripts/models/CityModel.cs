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
    public Action<CityElement> OnCityElementUnlocked;

    private List<CityElement> cityElements;
    private List<CityElementGroup> groups;

    private string currentGroupName;

    public CityElementGroup GetGroupByName(string groupName)
    {
        Assert.IsNotNull(this.groups, "CityModel GetGroupByName: groups list is null");
        Assert.IsTrue(this.groups.Count > 0, "CityModel GetGroupByName: groups list is empty");
        Assert.IsFalse(string.IsNullOrEmpty(groupName), "CityModel GetGroupByName: groupName should not be null or empty");
        return this.groups.Find((g) => g.GroupName == groupName);
    }

    public void SetGroups(List<CityElementGroup> groups, string currentGroupName)
    {
        Assert.IsNotNull(groups, "CityModel SetGroups: groups list is null");
        Assert.IsTrue(groups.Count > 0, "CityModel SetGroups: groups list is empty");
        Assert.IsFalse(string.IsNullOrEmpty(currentGroupName), "CityModel SetGroups: currentGroupName should not be null or empty");
        Debug.Log($"CityModel SetGroups: setting groups with current group name {currentGroupName}");
        this.groups = groups;
        this.SetCurrentGroupName(currentGroupName);
    }

    public void SetCurrentGroupName(string currentGroupName)
    {
        Debug.Log($"CityModel SetCurrentGroupName: setting current group name to {currentGroupName}");
        var group = this.groups.Find(g => g.GroupName == currentGroupName);
        Assert.IsNotNull(group, $"CityModel SetCurrentGroupName: failed to find group with name {currentGroupName}");
        this.cityElements = group.GetElements().ToList();
        this.currentGroupName = currentGroupName;
    }


    public List<string> GetAllGroupNames()
    {
        Assert.IsNotNull(this.groups, "CityModel GetAllGroupNames: groups list is null");
        Assert.IsTrue(this.groups.Count > 0, "CityModel GetAllGroupNames: groups list is empty");
        return this.groups.Select(g => g.GroupName).ToList();
    }

    public string GetNextGroupName()
    {
        Assert.IsNotNull(this.groups, "CityModel GetNextGroupName: groups list is null");
        Assert.IsTrue(this.groups.Count > 0, "CityModel GetNextGroupName: groups list is empty");
        var currentIndex = this.groups.FindIndex(g => g.GroupName == this.currentGroupName);
        Assert.IsTrue(currentIndex >= 0, $"CityModel GetNextGroupName: failed to find index of current group name {this.currentGroupName} in groups list");
        var nextIndex = (currentIndex + 1);
        if (nextIndex >= this.groups.Count)
        {
            return null;
        }
        return this.groups[nextIndex].GroupName;
    }

    public bool HasGroups()
    {
        return this.groups != null && this.groups.Count > 0;
    }

    public void FlyBrick(Vector3 from, Transform targetBrick, ColorIndex colorIndex = ColorIndex.Undefined)
    {
        OnFlyBrick?.Invoke(new FlyBrickData { from = from, targetBrick = targetBrick, colorIndex = colorIndex });
    }


    public void DeactivateAllElements()
    {
        Assert.IsNotNull(cityElements, "CityModel DeactivateAllElements: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel DeactivateAllElements: cityElements list is empty");
        foreach (var ce in cityElements)
        {
            ce.gameObject.SetActive(false);
        }
    }

    public void ActivateElements(int toIndex)
    {
        Assert.IsNotNull(cityElements, "CityModel ActivateElements: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel ActivateElements: cityElements list is empty");
        Assert.IsTrue(toIndex < cityElements.Count, $"CityModel ActivateElements: toIndex {toIndex} is out of range");
        for (var i = 0; i <= toIndex && i < cityElements.Count; i++)
        {
            cityElements[i].gameObject.SetActive(true);
            cityElements[i].EnableVisuals(true);
            cityElements[i].EnableBricks(false);

            if (i == toIndex)
                this.OnCityElementUnlocked?.Invoke(cityElements[toIndex]);
        }
        for (var i = toIndex + 1; i < cityElements.Count; i++)
        {
            cityElements[i].gameObject.SetActive(false);
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

    /*public CityElement UnlockElements(string dataKey)
    {
        for (var i = 0; i < cityElements.Count; i++)
        {
            var ce = cityElements[i];
            ce.gameObject.SetActive(true);
            if (ce.dataKey == dataKey)
            {
                return ce;
            }
        }
        Debug.LogError($"CityModel UnlockElement: failed to find city element with dataKey {dataKey}");
        return null;
    }*/


    public CityElement GetElementByIndex(int index)
    {
        Assert.IsNotNull(cityElements, "CityModel GetElementByIndex: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel GetElementByIndex: cityElements list is empty");
        Assert.IsTrue(index >= 0 && index < cityElements.Count, $"CityModel GetElementByIndex: index {index} is out of range");
        return cityElements[index];
    }
}