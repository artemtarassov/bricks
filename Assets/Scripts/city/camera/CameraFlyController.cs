using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using BezierSolution;
using System.Linq;
using System;

public class CameraFlyController : MonoBehaviour
{
    [SerializeField] public Vector3 camPos;
    [SerializeField] public Vector3 camRot;
    [SerializeField] private Transform skyLookAtTransform;
    private Camera cam => Camera.main;
    private Transform lookAt;
    private Vector3 lookAtChanged;
    private CameraTouchLookaround touchLookaround;
    private bool UseTouchLookaround
    {
        get
        {
#if UNITY_EDITOR
            return false;
#else
            return touchLookaround != null;
#endif
        }
    }


    void Start()
    {

        touchLookaround = this.gameObject.GetComponent<CameraTouchLookaround>();
        if (UseTouchLookaround)
        {
            touchLookaround.enabled = false;
        }

        CamModel.Instance.OnMoveCameraToBuilding += OnMoveCameraToBuilding;
        CamModel.Instance.OnMoveCameraToCityElement += OnMoveCameraToCityElement;
        CamModel.Instance.OnAnticipateRocketFly += OnAnticipateRocketFly;
        CamModel.Instance.OnMoveCamBack += OnMoveCamBack;
        CamModel.Instance.OnMoveCameraToSky += OnMoveCameraToSky;

        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        if (progress != null)
        {
            OnMoveCameraToBuilding();
        }
    }

    private void OnMoveCameraToSky()
    {
        Debug.Log($"CameraFlyController OnMoveCameraToSky: moving camera to sky");
        var spline = this.GetComponentsInChildren<BezierSpline>(true).ToList().Find((s) => s.gameObject.name == "Sky");
        this.lookAt = this.skyLookAtTransform;
        MoveCameraLongSpline(spline, Durations.CamOrbit);
        if (UseTouchLookaround)
        {
            touchLookaround.enabled = false;
        }
    }

    void OnDestroy()
    {
        CamModel.Instance.OnMoveCameraToBuilding -= OnMoveCameraToBuilding;
        CamModel.Instance.OnMoveCameraToCityElement -= OnMoveCameraToCityElement;
        CamModel.Instance.OnAnticipateRocketFly -= OnAnticipateRocketFly;
        CamModel.Instance.OnMoveCamBack -= OnMoveCamBack;
        CamModel.Instance.OnMoveCameraToSky -= OnMoveCameraToSky;
    }

    private void OnMoveCamBack()
    {

        cam.transform.DOKill();

        //move cam a bit away and up to show the rocket fly better
        var toPos = cam.transform.position + (cam.transform.forward * -1);
        var t = 0.464f;
        cam.transform.DOMove(toPos, t).SetEase(Ease.OutBack);
    }

    private void OnAnticipateRocketFly()
    {
        cam.transform.DOKill();

        //move cam a bit away and up to show the rocket fly better
        var toPos = cam.transform.position + (cam.transform.forward * -5);
        var t = Durations.RocketFlyDuration;
        cam.transform.DOMove(toPos, t).SetEase(Ease.OutSine);
        DOVirtual.DelayedCall(Durations.RocketFlyDuration - 0.15f, ShakeExplision);
    }

    private void ShakeExplision()
    {
        var t = 0.3f;
        cam.transform.DOShakePosition(t, 0.1f);
    }


    private void OnMoveCameraToCityElement(CityElement cityElement)
    {
        Debug.Log($"CameraFlyController OnMoveCameraToCityElement: moving camera to city element {cityElement.name}");
        cam.transform.DOKill();

        var t = Durations.CamFly;
        cam.transform.DOMove(cityElement.camPos, t)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                if (UseTouchLookaround)
                {
                    touchLookaround.Setup(cityElement.camPos, cityElement.camRot);
                }
            });
        cam.transform.DORotate(cityElement.camRot, t * 0.8f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            if (UseTouchLookaround)
            {
                touchLookaround.enabled = true;
                touchLookaround.Setup(cityElement.camPos, cityElement.camRot);
            }
        });
    }


    private int prevBuildingIndex = -1;

    private void OnMoveCameraToBuilding()
    {
        var currentBuildingName = PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName;
        Debug.Log($"CameraFlyController OnMoveCameraToBuilding: moving camera to building {currentBuildingName}");
        var currentBuilding = CityModel.Instance.GetBuildingByName(currentBuildingName);
        var spline = this.GetSpline(currentBuildingName);
        this.lookAt = currentBuilding.GetCamCenterPos();

        var buildingIndex = BuildingNameUtil.allBuildingNamesRegular.IndexOf(currentBuildingName);
        if (buildingIndex == -1)
        {
            MoveCameraLongSpline(spline, Durations.CamOrbit);
            return;
        }

        if(prevBuildingIndex == -1)
        {
            prevBuildingIndex = buildingIndex;
            MoveCameraLongSpline(spline, Durations.CamOrbit);
            return;
        }

        var direction = buildingIndex > prevBuildingIndex ? -1 : 1;
        prevBuildingIndex = buildingIndex;
        MoveCameraFromDirection(spline, direction, () =>
        {
            MoveCameraLongSpline(spline, Durations.CamOrbit);
        });
    }

    private BezierSpline GetSpline(BuildingName currentBuildingName)
    {
        var splineName = currentBuildingName.ToString();
        if (BuildingNameUtil.IsChallengeBuilding(currentBuildingName))
        {
            splineName = "Challenges";
        }
        var spline = this.GetComponentsInChildren<BezierSpline>(true).ToList().Find((s) => s.gameObject.name == splineName);
        Assert.IsNotNull(spline, "CameraFlyController OnMoveCameraToBuilding: no spline found for building " + currentBuildingName);
        return spline;
    }



    private Tween tween1;
    private void MoveCameraLongSpline(BezierSpline spline, float duration)
    {
        if (UseTouchLookaround)
        {
            touchLookaround.enabled = false;
        }

        if (tween1 != null)
        {
            tween1.Kill();
        }

        cam.transform.DOKill();
        cam.transform.position = spline.GetPoint(0);
        cam.transform.LookAt(lookAt);
        lookAtChanged = lookAt.position;

        tween1 = DOTween.To(
            () => 0f,
            progress =>
            {
                cam.transform.position = spline.GetPoint(progress);
                cam.transform.LookAt(lookAtChanged);
            },
            1f,
            duration
        ).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetTarget(cam.transform);

    }

    private void MoveCameraFromDirection(BezierSpline spline, int fromDirection, TweenCallback OnComplete)
    {
        cam.transform.DOKill();
        cam.transform.position = spline.GetPoint(0);
        cam.transform.LookAt(lookAt.position + new Vector3(4 * fromDirection, 0, 0));
        cam.transform.DOLookAt(lookAt.position, 0.2f).SetEase(Ease.OutSine).OnComplete(OnComplete);
    }


}
