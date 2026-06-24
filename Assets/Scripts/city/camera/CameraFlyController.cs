using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using BezierSolution;
using System.Linq;

public class CameraFlyController : MonoBehaviour
{
    [SerializeField] public Vector3 camPos;
    [SerializeField] public Vector3 camRot;
    [SerializeField] private Transform skyLookAtTransform;
    private Transform lookAt;
    private Vector3 lookAtChanged;
    private CameraTouchLookaround touchLookaround;

    void Start()
    {
        touchLookaround = Camera.main.GetComponent<CameraTouchLookaround>();
        if (touchLookaround == null)
        {
            touchLookaround = Camera.main.gameObject.AddComponent<CameraTouchLookaround>();
        }
        CamModel.Instance.OnMoveCameraToBuilding += OnMoveCameraToBuilding;
        CamModel.Instance.OnMoveCameraToCityElement += OnMoveCameraToCityElement;
        CamModel.Instance.OnAnticipateRocketFly += OnAnticipateRocketFly;
        CamModel.Instance.OnMoveCamBack += OnMoveCamBack;
        CamModel.Instance.OnMoveCameraToSky += OnMoveCameraToSky;


        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        if (progress.State == BuildingState.Unlocked || progress.State == BuildingState.Completed)
        {
            OnMoveCameraToBuilding();
        }
    }

    private void OnMoveCameraToSky()
    {
        var spline = this.GetComponentsInChildren<BezierSpline>(true).ToList().Find((s) => s.gameObject.name == "Sky");
        this.lookAt = this.skyLookAtTransform;
        MoveCameraLongSpline(spline, Durations.CamOrbit);
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
        var mainCam = Camera.main;
        mainCam.transform.DOKill();

        //move cam a bit away and up to show the rocket fly better
        var toPos = mainCam.transform.position + (mainCam.transform.forward * -1);
        var t = 0.464f;
        mainCam.transform.DOMove(toPos, t).SetEase(Ease.OutBack);
    }

    private void OnAnticipateRocketFly()
    {
        var mainCam = Camera.main;
        mainCam.transform.DOKill();

        //move cam a bit away and up to show the rocket fly better
        var toPos = mainCam.transform.position + (mainCam.transform.forward * -5);
        var t = Durations.RocketFlyDuration;
        mainCam.transform.DOMove(toPos, t).SetEase(Ease.OutSine);
        DOVirtual.DelayedCall(Durations.RocketFlyDuration - 0.15f, ShakeExplision);
    }

    private void ShakeExplision()
    {
        var mainCam = Camera.main;
        var t = 0.3f;
        mainCam.transform.DOShakePosition(t, 0.1f);
    }


    private void OnMoveCameraToCityElement(CityElement cityElement)
    {
        var mainCam = Camera.main;
        mainCam.transform.DOKill();

        var t = Durations.CamFly;
        mainCam.transform.DOMove(cityElement.camPos, t)
            .SetEase(Ease.OutBack)
            .OnComplete(() => touchLookaround?.Setup(cityElement.camPos, cityElement.camRot));
        mainCam.transform.DORotate(cityElement.camRot, t * 0.8f).SetEase(Ease.OutSine);
    }

    private void OnMoveCameraToBuilding()
    {
        var currentBuildingName = PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName;
        var currentBuilding = CityModel.Instance.GetBuildingByName(currentBuildingName);
        var spline = this.GetComponentsInChildren<BezierSpline>(true).ToList().Find((s) => s.gameObject.name == currentBuildingName.ToString());
        Assert.IsNotNull(spline, "CameraFlyController OnMoveCameraToBuilding: no spline found for building " + currentBuildingName);
        this.lookAt = currentBuilding.GetCamCenterPos();
        MoveCameraLongSpline(spline, Durations.CamOrbit);
    }


    private void MoveCameraLongSpline(BezierSpline spline, float duration)
    {
        var cam = Camera.main;
        cam.transform.DOKill();
        cam.transform.position = spline.GetPoint(0);
        cam.transform.LookAt(lookAt);

        DOTween.To(
            () => 0f,
            progress =>
            {
                lookAtChanged = (lookAt.position + new Vector3(0, 3.0f - progress * 6, 0));
            },
            1f,
            duration / 2
        ).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetTarget(cam.transform);


        DOTween.To(
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


}
