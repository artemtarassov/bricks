using System;
using System.Collections.Generic;
using UnityEngine;

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
    public static readonly CityModel Instance = new CityModel();

    public Action<FlyData> OnFlyBrick;
    public Action<CityElement> OnCityElementUnlocked;

    private List<CityElement> cityElements = new List<CityElement>();

    public void AddCityElement(CityElement element)
    {
        this.cityElements.Add(element);
    }

    public void FlyBrick(Vector3 from, Transform targetBrick, ColorIndex colorIndex = ColorIndex.Undefined)
    {
        OnFlyBrick?.Invoke(new FlyData { from = from, targetBrick = targetBrick, colorIndex = colorIndex });
    }

    public CityElement UnlockNextElement()
    {
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
        return cityElements.FindLast(e => e.gameObject.activeSelf);
    }
} 