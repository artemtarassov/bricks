using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;


public class RocketController : MonoBehaviour
{
    [SerializeField] private Transform rocket;
    private Vector3 targetPos;
    private Vector3 fromPos;
    private Vector3 movedPos;

    private float progress;
    void Start()
    {
        ViewModel.Instance.OnFlyRocket += FlyRocket;

    }
    void OnDestroy()
    {
        ViewModel.Instance.OnFlyRocket -= FlyRocket;
    }

    private void FlyRocket(Vector3 fromFlyPos, Vector3 toFlyPos)
    {
        this.fromPos = fromFlyPos;
        this.targetPos = toFlyPos;


        this.rocket.position = fromFlyPos;
        this.rocket.gameObject.SetActive(true);
        this.rocket.transform.LookAt(toFlyPos);

        var t = Durations.RocketFlyDuration;

        rocket.DOKill();
        this.rocket.DOMove(toFlyPos, t).SetEase(Ease.InQuad).OnComplete(() =>
        {
            this.rocket.gameObject.SetActive(false);
        });
        var inner = this.rocket.GetChild(0);
        inner.DOKill();
        var rot = inner.localEulerAngles;
        inner.DOLocalRotate(new Vector3(rot.x + 180, rot.y + 360, rot.z), t, RotateMode.FastBeyond360).SetEase(Ease.Linear);
    }

}