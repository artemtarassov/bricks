using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class FlyData
{
    public Vector3 from;
    public Transform targetBrick;
    public ColorIndex colorIndex;
}

public class BricksLayer
{
    public int layerIndex;
    public List<Transform> bricks;
    public float yPos;
}

public class CityModel
{
    public static CityModel Instance;

    public Action<FlyData> OnFlyBrick;
    public Action<CityElement> OnCityElementUnlocked;

    private List<CityElement> cityElements;
    private List<CityElementGroup> groups;

    public void SetGroups(List<CityElementGroup> groups, string currentGroupName)
    {
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
    }

    public bool HasGroups()
    {
        return this.groups != null && this.groups.Count > 0;
    }

    public void FlyBrick(Vector3 from, Transform targetBrick, ColorIndex colorIndex = ColorIndex.Undefined)
    {
        OnFlyBrick?.Invoke(new FlyData { from = from, targetBrick = targetBrick, colorIndex = colorIndex });
    }

    public CityElement UnlockElement(string dataKey)
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
    }

    public CityElement UnlockNextElement()
    {
        Assert.IsNotNull(cityElements, "CityModel UnlockNextElement: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel UnlockNextElement: cityElements list is empty");
        var nextElement = cityElements.Find(e => e.gameObject.activeSelf == false);
        if (nextElement != null)
        {
            nextElement.gameObject.SetActive(true);
            OnCityElementUnlocked?.Invoke(nextElement);
        }
        return nextElement;
    }

    public CityElement GetCurrentElement()
    {
        Assert.IsNotNull(cityElements, "CityModel GetCurrentElement: cityElements list is null");
        Assert.IsTrue(cityElements.Count > 0, "CityModel GetCurrentElement: cityElements list is empty");
        return cityElements.FindLast(e => e.gameObject.activeSelf);
    }
}