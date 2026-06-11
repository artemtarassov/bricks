using System;
using UnityEngine;

public class CamModel
{
    public static CamModel Instance;

    public Action OnMoveCameraToElementGroup;
    public Action<CityElement> OnMoveCameraToCityElement;

    public Action OnAnticipateRocketFly;
    public Action OnMoveCamBack;


    public void AnticipateRocketFly()
    {
        Debug.Log("CamModel AnticipateRocketFly: anticipating rocket fly");
        OnAnticipateRocketFly?.Invoke();
    }

    public void MoveCamBack()
    {
        Debug.Log("CamModel MoveCamBack: moving camera back to default position");
        OnMoveCamBack?.Invoke();
    }

    public void MoveCameraToCityElement(CityElement cityElement)
    {
        Debug.Log("CamModel MoveCameraToCityElement: moving camera to city element " + cityElement.name);
        OnMoveCameraToCityElement?.Invoke(cityElement);
    }

    public void MoveCameraToElementGroup()
    {
        Debug.Log("CamModel MoveCameraToElementGroup: moving camera to element group");
        OnMoveCameraToElementGroup?.Invoke();
    }

}