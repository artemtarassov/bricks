using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BuildingElement : MonoBehaviour
{
    private BuildingName buildingName = BuildingName.Undefined;
    public BuildingName BuildingName
    {
        get
        {
            if (buildingName == BuildingName.Undefined)
            {
                buildingName = BuildingNameUtil.GetBuildingNameByString(this.gameObject.name);
            }
            return buildingName;
        }
    }
    private HashSet<CityElement> elements;

    private Transform centerObj;

    public CityElement GetElementByDataKey(string dataKey)
    {
        var elements = GetElements();
        foreach (var e in elements)
        {
            if (e.dataKey == dataKey)
            {
                return e;
            }
        }
        return null;
    }

    public HashSet<CityElement> GetElements()
    {
        if (elements == null)
        {
            elements = new HashSet<CityElement>(this.GetComponentsInChildren<CityElement>(true));
        }
        Assert.IsTrue(elements.Count > 0, $"BuildingElement {BuildingName} has no CityElements");
#if UNITY_EDITOR
        //ensure that all elements have unique data keys
        var dataKeys = new HashSet<string>();
        foreach (var e in elements)
        {
            if (dataKeys.Contains(e.dataKey))
            {
                Debug.LogError($"BuildingElement {BuildingName} has duplicate data key {e.dataKey} in element {e.gameObject.name}");
            }
            else
            {
                dataKeys.Add(e.dataKey);
            }
        }

#endif
        return elements;
    }

    public Transform GetCamCenterPos()
    {
        if (centerObj == null)
        {
            centerObj = this.transform.Find("CamCenter");
        }
        return centerObj != null ? centerObj : this.transform;
    }


}