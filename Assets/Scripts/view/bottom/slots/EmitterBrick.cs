using DG.Tweening;
using UnityEngine;

public class EmitterBrick : MonoBehaviour
{
    [HideInInspector]
    public BrickData brickData;

    private UIBrick uiBrick;

    void Awake()
    {
        this.uiBrick = this.GetComponentInChildren<UIBrick>(true);
    }

    public void Setup()
    {
        this.brickData = null;
        this.uiBrick.SetData(null);
        this.uiBrick.transform.DOKill();
        this.uiBrick.transform.localScale = Vector3.one;
    }

    public void Setup(BrickData eb, bool animate)
    {
        this.brickData = eb;
        this.uiBrick.SetData(eb);
        this.uiBrick.transform.DOKill();
        if (animate)
        {
            this.uiBrick.transform.localScale = Vector3.zero;
            this.uiBrick.transform.DOScale(Vector3.one, Durations.SlotElementMove).SetEase(Ease.OutBack);
        }
        else
        {
            this.uiBrick.transform.localScale = Vector3.one;
        }
    }
}