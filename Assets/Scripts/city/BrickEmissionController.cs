using DG.Tweening;
using UnityEngine;

public class BrickEmissionController : MonoBehaviour
{
    private Sequence update;
    void Start()
    {
        SlotModel.Instance.OnBrickMovedFromColumnToEmitter += OnBrickMovedFromColumnToEmitter;
    }

    void OnDestroy()
    {
        SlotModel.Instance.OnBrickMovedFromColumnToEmitter -= OnBrickMovedFromColumnToEmitter;
        if (this.update != null)
        {
            this.update.Kill();
            this.update = null;
        }
    }
    private void OnBrickMovedFromColumnToEmitter(BrickData bd, int emitterIndex)
    {
        this.StartUpdate(true);
    }

    private void StartUpdate(bool start)
    {
        if (start)
        {
            if (this.update != null)
            {
                return;
            }
            this.update = DOTween.Sequence(this).AppendInterval(0.1f).AppendCallback(OnSecUpdate).SetLoops(-1);
        }
        else
        {
            if (this.update != null)
            {
                this.update.Kill();
                this.update = null;
            }
        }
    }

    void OnSecUpdate()
    {
        var hasBricksInEmitters = SlotModel.Instance.HasBricksInEmitters();
        if (hasBricksInEmitters)
        {
            new EmitBricksCmd().Run();
        }
        else
        {
            this.StartUpdate(false);
        }
    }
}