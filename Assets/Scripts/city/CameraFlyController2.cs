using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using BezierSolution;

public class CameraFlyController2 : MonoBehaviour
{
    public static float SplineAlignDuration = 3f;
    public static float CityElementTransitionDuration = 3f;
    public static float SplineRiseAmount = 10f;
    public static float SplineRotationBlendDuration = 0.75f;

    [SerializeField] public Vector3 camPos;
    [SerializeField] public Vector3 camRot;
    private Transform lookAt;
    private Vector3 lookAtChanged;
    private BezierSpline activeSpline;
    private float currentSplineT;
    private bool isOrbitingOnSpline;

    private CityElement currentCityElement;

    void Start()
    {
        CamModel.Instance.OnMoveCameraToBuilding += OnMoveCameraToBuilding;
        CamModel.Instance.OnMoveCameraToCityElement += OnMoveCameraToCityElement;

        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        if (progress.State == BuildingState.Unlocked || progress.State == BuildingState.Completed)
        {
            OnMoveCameraToBuilding();
        }
    }

    void OnDestroy()
    {
        CamModel.Instance.OnMoveCameraToBuilding -= OnMoveCameraToBuilding;
        CamModel.Instance.OnMoveCameraToCityElement -= OnMoveCameraToCityElement;
    }

    private void OnMoveCameraToCityElement(CityElement cityElement)
    {
        currentCityElement = cityElement;
        var mainCam = Camera.main;
        mainCam.transform.DOKill();


        if (isOrbitingOnSpline && activeSpline != null && cityElement.camPos != Vector3.zero)
        {
            MoveCameraToClosestSplinePoint(mainCam.transform, cityElement.camPos);
            return;
        }

        var hasStoredCameraPose = cityElement.camPos != Vector3.zero && cityElement.camRot != Vector3.zero;
        var targetPosition = hasStoredCameraPose
            ? cityElement.camPos
            : cityElement.GetAveragePosition() + new Vector3(20, 10, 20);

        StartCityElementTransition(mainCam.transform, cityElement, targetPosition, hasStoredCameraPose);
    }

    private void StartCityElementTransition(Transform cameraTransform, CityElement cityElement, Vector3 targetPosition, bool hasStoredCameraPose)
    {
        isOrbitingOnSpline = false;

        if (!hasStoredCameraPose)
        {
            var p = cityElement.GetAveragePosition();
            cameraTransform.DOMove(targetPosition, CityElementTransitionDuration).OnUpdate(() =>
            {
                cameraTransform.LookAt(p);
            });
        }
        else
        {
            cameraTransform.DOMove(targetPosition, CityElementTransitionDuration).SetEase(Ease.InOutSine);
            TweenRotationShortestPath(cameraTransform, cityElement.camRot, CityElementTransitionDuration);
        }
    }

    private void MoveCameraToClosestSplinePoint(Transform cameraTransform, Vector3 targetPosition)
    {
        activeSpline.FindNearestPointTo(targetPosition, out var targetSplineT, 1000f);
        var startT = currentSplineT;
        var endT = GetClosestTravelT(startT, targetSplineT, activeSpline.loop);
        var currentMoveT = startT;
        var currentYOffset = 0f;
        var currentRotationBlend = 0f;
        var startRotation = cameraTransform.rotation;
        isOrbitingOnSpline = false;

        void ApplySplineMove()
        {
            currentSplineT = NormalizeSplineT(currentMoveT, activeSpline.loop);
            var currentPosition = activeSpline.GetPoint(currentSplineT) + new Vector3(0f, currentYOffset, 0f);
            cameraTransform.position = currentPosition;
            if (lookAt != null)
            {
                var lookTarget = lookAtChanged == Vector3.zero ? lookAt.position : lookAtChanged;
                var lookDirection = lookTarget - currentPosition;
                if (lookDirection.sqrMagnitude > 0.0001f)
                {
                    var targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                    cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, currentRotationBlend);
                }
            }
        }

        var sequence = DOTween.Sequence().SetTarget(cameraTransform);
        sequence.Join(DOTween.To(
            () => currentMoveT,
            splineT =>
            {
                currentMoveT = splineT;
                ApplySplineMove();
            },
            endT,
            SplineAlignDuration
        ).SetEase(Ease.InOutSine));
        sequence.Join(DOTween.To(
            () => currentYOffset,
            yOffset =>
            {
                currentYOffset = yOffset;
                ApplySplineMove();
            },
            SplineRiseAmount,
            SplineAlignDuration
        ).SetEase(Ease.Linear));
        sequence.Join(DOTween.To(
            () => currentRotationBlend,
            rotationBlend =>
            {
                currentRotationBlend = rotationBlend;
                ApplySplineMove();
            },
            1f,
            Mathf.Max(0.01f, SplineRotationBlendDuration)
        ).SetEase(Ease.OutSine));
    }

    private void OnMoveCameraToBuilding()
    {
        var currentBuildingName = PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName;
        var currentBuilding = CityModel.Instance.GetBuildingByName(currentBuildingName);
        var spline = this.GetComponentsInChildren<BezierSpline>(true).ToList().Find((s) => s.gameObject.name == currentBuildingName.ToString());
        Assert.IsNotNull(spline, "CameraFlyController2 OnMoveCameraToBuilding: no spline found for building " + currentBuildingName);
        var cam = Camera.main;
        cam.transform.DOKill();
        var duration = 200;
        activeSpline = spline;
        isOrbitingOnSpline = true;
        this.lookAt = currentBuilding.GetCamCenterPos();
        MoveCameraLongSpline(spline, duration);
    }

    private void MoveCameraLongSpline(BezierSpline spline, float duration)
    {
        var cam = Camera.main;
        currentSplineT = 0f;
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
                currentSplineT = NormalizeSplineT(progress, spline.loop);
                cam.transform.position = spline.GetPoint(currentSplineT);
                cam.transform.LookAt(lookAtChanged);
            },
            1f,
            duration
        ).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetTarget(cam.transform);
    }

    private static Tween TweenRotationShortestPath(Transform target, Vector3 targetEulerAngles, float duration)
    {
        var startRotation = target.rotation;
        var targetRotation = Quaternion.Euler(targetEulerAngles);
        return DOTween.To(
            () => 0f,
            progress => target.rotation = Quaternion.Slerp(startRotation, targetRotation, progress),
            1f,
            duration
        ).SetEase(Ease.InOutSine).SetTarget(target);
    }

    private static float NormalizeSplineT(float normalizedT, bool splineLoops)
    {
        if (!splineLoops)
        {
            return Mathf.Clamp01(normalizedT);
        }

        normalizedT %= 1f;
        if (normalizedT < 0f)
        {
            normalizedT += 1f;
        }

        return normalizedT;
    }

    private static float GetClosestTravelT(float currentT, float targetT, bool splineLoops)
    {
        if (!splineLoops)
        {
            return Mathf.Clamp01(targetT);
        }

        var directDistance = Mathf.Abs(targetT - currentT);
        var forwardWrapDistance = Mathf.Abs((targetT + 1f) - currentT);
        var backwardWrapDistance = Mathf.Abs((targetT - 1f) - currentT);

        if (forwardWrapDistance < directDistance && forwardWrapDistance <= backwardWrapDistance)
        {
            return targetT + 1f;
        }

        if (backwardWrapDistance < directDistance)
        {
            return targetT - 1f;
        }

        return targetT;
    }
}
