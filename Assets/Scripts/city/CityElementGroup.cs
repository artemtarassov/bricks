using System.Collections.Generic;
using UnityEngine;

public class CityElementGroup : MonoBehaviour
{
    public string GroupName => this.gameObject.name;
    private HashSet<CityElement> elements;

    public HashSet<CityElement> GetElements()
    {
        if (elements == null)
        {
            elements = new HashSet<CityElement>(this.GetComponentsInChildren<CityElement>(true));
        }
#if UNITY_EDITOR
        //ensure that all elements have unique data keys
        var dataKeys = new HashSet<string>();
        foreach (var e in elements)
        {
            if (dataKeys.Contains(e.dataKey))
            {
                Debug.LogError($"CityElementGroup {GroupName} has duplicate data key {e.dataKey} in element {e.gameObject.name}");
            }
            else
            {
                dataKeys.Add(e.dataKey);
            }
        }

#endif
        return elements;
    }


}