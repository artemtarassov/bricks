using DG.Tweening;
using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    [SerializeField] private GameObject rotatingCircle;
    private Sequence rotationSequence;

    void OnEnable()
    {
        if (rotationSequence != null)
            rotationSequence.Kill();
        //rotate circle stepwise by 45 degrees every 0.1 seconds
        rotationSequence = DOTween.Sequence(this).AppendInterval(0.05f)
            .AppendCallback(RotateCircle)
            .SetLoops(-1);
    }

    private void RotateCircle()
    {
        rotatingCircle.transform.Rotate(0, 0, -45);
    }

    void OnDisable()
    {
        if (rotationSequence != null)
            rotationSequence.Kill();
    }
}