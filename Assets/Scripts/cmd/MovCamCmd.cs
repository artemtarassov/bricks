using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;


public class MovCamCmd
{
    public void Run(CityElement cityElement)
    {
        var mainCam = Camera.main;
        if (cityElement.camPos == Vector3.zero || cityElement.camRot == Vector3.zero)
        {
            var p = cityElement.GetAveragePosition();
            mainCam.transform.DOMove(p + new Vector3(20, 10, 20), 1f).OnUpdate(() =>
            {
                mainCam.transform.LookAt(p);
            });
        }
        else
        {
            mainCam.transform.DOMove(cityElement.camPos, 1f);
            mainCam.transform.DORotate(cityElement.camRot, 1f);
        }
    }

}