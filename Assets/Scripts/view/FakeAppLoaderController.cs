using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class FakeAppLoaderController : MonoBehaviour
{
#if UNITY_EDITOR
    void Awake()
    {
        Destroy(this.gameObject);
    }
#endif
    void Start()
    {
        var isFirstStart = RemoteConfigData.HasSavedData() == false;
        if (isFirstStart)
        {
            DOVirtual.DelayedCall(Durations.FakeLoaderDuration, OnComplete);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
    private void OnComplete()
    {
        var loadingCircle = this.gameObject.GetComponentInChildren<LoadingCircle>();
        if (loadingCircle != null)
        {
            loadingCircle.transform.DOScale(0, 0.33f).SetEase(Ease.InBack);
        }
        var canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
        canvasGroup.DOFade(0, 0.5f).SetDelay(0.33f).OnComplete(() =>
        {
            GameObject.Destroy(this.gameObject);
        });
    }

}