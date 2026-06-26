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
        touchLookaround = this.gameObject.GetComponent<CameraTouchLookaround>();
        touchLookaround.enabled = false;

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
        touchLookaround.enabled = false;
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
        Debug.Log($"CameraFlyController OnMoveCameraToCityElement: moving camera to city element {cityElement.name}");
        var mainCam = Camera.main;
        mainCam.transform.DOKill();

        var t = Durations.CamFly;
        mainCam.transform.DOMove(cityElement.camPos, t)
            .SetEase(Ease.OutBack)
            .OnComplete(() => touchLookaround?.Setup(cityElement.camPos, cityElement.camRot));
        mainCam.transform.DORotate(cityElement.camRot, t * 0.8f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            touchLookaround.enabled = true;
            touchLookaround?.Setup(cityElement.camPos, cityElement.camRot);
        });
    }

    private void OnMoveCameraToBuilding()
    {
        var currentBuildingName = PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName;
        Debug.Log($"CameraFlyController OnMoveCameraToBuilding: moving camera to building {currentBuildingName}");
        var currentBuilding = CityModel.Instance.GetBuildingByName(currentBuildingName);
        var spline = this.GetSpline(currentBuildingName);
        this.lookAt = currentBuilding.GetCamCenterPos();
        MoveCameraLongSpline(spline, Durations.CamOrbit);
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


    private void MoveCameraLongSpline(BezierSpline spline, float duration)
    {
        touchLookaround.enabled = false;

        var cam = Camera.main;
        cam.transform.DOKill();
        cam.transform.position = spline.GetPoint(0);
        cam.transform.LookAt(lookAt);

        var yShift = 2.0f;

        DOTween.To(
            () => 0f,
            progress =>
            {
                lookAtChanged = (lookAt.position + new Vector3(0, (yShift / 2.0f) - progress * yShift, 0));
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
