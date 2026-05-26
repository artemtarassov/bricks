using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class BtnAddSpaceAd : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

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
        if (isReady)
        {
            this.text.text = "Watch Ad";
        }
        else
        {
            this.text.text = "Loading";
        }
    }

    void OnDisable()
    {
        if (updateSequence != null)
            updateSequence.Kill();
    }
}