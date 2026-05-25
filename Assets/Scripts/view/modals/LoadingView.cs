using DG.Tweening;
using UnityEngine;

public class LoadingView : DefaultView
{
    [SerializeField] private GameObject rotatingCircle;

    public override void OnShown(bool animate)
    {
        //rotate circle stepwise by 45 degrees every 0.1 seconds
        DOTween.Sequence(this).AppendInterval(0.05f)
            .AppendCallback(RotateCircle)
            .SetLoops(-1);
    }

    private void RotateCircle()
    {
        rotatingCircle.transform.Rotate(0, 0, -45);
    }

    public override void OnHidden()
    {
        DOTween.Kill(this);
    }
}