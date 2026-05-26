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
        return elements;
    }


}