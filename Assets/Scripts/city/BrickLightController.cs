using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class BrickLightController : MonoBehaviour
{
    private Light lightComponent;//point light.
    void Start()
    {
        this.lightComponent = this.GetComponentInChildren<Light>(true);
        Assert.IsNotNull(this.lightComponent, $"BrickLightController Start: failed to find Light component on {this.name}");
        Assert.IsTrue(this.lightComponent.type == LightType.Point, $"BrickLightController Start: expected Light component to be of type Point on {this.name}");
        this.lightComponent.gameObject.SetActive(false);
        Assert.IsNotNull(this.lightComponent, $"BrickLightController Start: failed to find Light component on {this.name}");
        CityModel.Instance.OnCityElementUnlocked += OnCityElementUnlocked;

        if (CityModel.Instance.HasGroups())
        {
            OnCityElementUnlocked(CityModel.Instance.GetCurrentElement());
        }
    }

    private void OnCityElementUnlocked(CityElement cityElement)
    {
        this.lightComponent.gameObject.SetActive(true);
        var sourcePos = cityElement.GetAveragePosition();
        var camPos = cityElement.camPos;
        Assert.IsTrue(camPos != null, $"BrickLightController OnCityElementUnlocked: camPos should not be null for city element {cityElement.name}");
        Assert.IsTrue(sourcePos != null, $"BrickLightController OnCityElementUnlocked: sourcePos should not be null for city element {cityElement.name}");
        Assert.IsTrue(Vector3.Distance(camPos, sourcePos) > 1.0f, $"BrickLightController OnCityElementUnlocked: camPos {camPos} and sourcePos {sourcePos} should not be the same for city element {cityElement.name}");
        Assert.AreNotEqual(sourcePos,Vector3.zero, $"BrickLightController OnCityElementUnlocked: sourcePos should not be Vector3.zero for city element {cityElement.name}");
        Assert.AreNotEqual(camPos, Vector3.zero, $"BrickLightController OnCityElementUnlocked: camPos should not be Vector3.zero for city element {cityElement.name}");
        //position the light 10 units away from the source in the direction of the camera
        var direction = (camPos - sourcePos).normalized;
        var targetPos = sourcePos + direction * 5f;
        this.lightComponent.transform.position = targetPos;
    }
    void OnDestroy()
    {
        CityModel.Instance.OnCityElementUnlocked -= OnCityElementUnlocked;
    }

}