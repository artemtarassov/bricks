using System.Collections;
using DG.Tweening;
using UnityEngine;

public class AdLoadingCircle : MonoBehaviour
{
    [SerializeField] private GameObject rotatingCircle;
    [SerializeField] private GameObject ready;

    private Tween updateSequence;

    void OnEnable()
    {
        if (updateSequence != null)
            updateSequence.Kill();

        this.OnSecUpdate();

        updateSequence = DOTween.Sequence(this).AppendInterval(1)
            .AppendCallback(OnSecUpdate)
            .SetLoops(-1);
    }

    private void OnSecUpdate()
    {
        var isReady = AdModel.Instance.IsAdReady(AdUnits.Rewarded);
        this.rotatingCircle.SetActive(!isReady);
        this.ready.SetActive(isReady);
    }

    void OnDisable()
    {
        if (updateSequence != null)
            updateSequence.Kill();
    }
}