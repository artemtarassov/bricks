using UnityEngine;
using UnityEngine.Assertions;

public class BrickLightController2 : MonoBehaviour
{

    private Light lightComponent;//directional light.


    void Start()
    {
        this.lightComponent = this.GetComponentInChildren<Light>(true);
        Assert.IsNotNull(this.lightComponent, $"BrickLightController Start: failed to find Light component on {this.name}");
        Assert.IsTrue(this.lightComponent.type == LightType.Directional, $"BrickLightController Start: expected Light component to be of type Directional on {this.name}");
        this.lightComponent.gameObject.SetActive(false);
        Assert.IsNotNull(this.lightComponent, $"BrickLightController Start: failed to find Light component on {this.name}");
        CamModel.Instance.OnMoveCameraToCityElement += ShowLightForCityElement;
        CityModel.Instance.OnEnableDifferentColors += ShowLightForCityElement;//update light position when new colors are enabled, as it may change which brick is lit.
    }

    private void ShowLightForCityElement(CityElement cityElement)
    {
        this.lightComponent.gameObject.SetActive(true);
        if (cityElement.lightRot == Vector3.zero)
        {
            Debug.Log($"BrickLightController ShowLightForCityElement: lightRot is Vector3.zero for city element {cityElement.name}. Defaulting to look at city element average position.");
            this.lightComponent.transform.position = cityElement.camPos;
            this.lightComponent.transform.LookAt(cityElement.GetAveragePosition());
        }
        else
        {
            Debug.Log($"BrickLightController ShowLightForCityElement: setting light rotation to {cityElement.lightRot} for city element {cityElement.name}");
            this.lightComponent.transform.eulerAngles = cityElement.lightRot;
        }
    }

}