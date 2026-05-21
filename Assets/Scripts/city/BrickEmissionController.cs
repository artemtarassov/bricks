using DG.Tweening;
using UnityEngine;

public class BrickEmissionController : MonoBehaviour
{
    void Start()
    {
        DOTween.Sequence(this).AppendInterval(0.1f).AppendCallback(OnSecUpdate).SetLoops(-1);
    }

    void OnDestroy()
    {
        DOTween.Kill(this);
    }

    void OnSecUpdate()
    {
        new EmitBricksCmd().Run();
    }
}