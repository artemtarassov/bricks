using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmitterBrick : MonoBehaviour
{
    [HideInInspector]
    public BrickData brickData;

    [SerializeField] private TMP_Text count;
    [SerializeField] private Image colorImg;

    void Awake()
    {
        this.colorImg.transform.localScale = Vector3.zero;
        this.count.text = "";
    }


    public void Setup(BrickData eb = null, bool animate = false)
    {
        this.brickData = eb;

        if (eb == null)
        {
            this.count.text = "";
            this.colorImg.transform.DOKill();
            if (animate)
            {
                this.colorImg.transform.DOScale(Vector3.zero, Durations.SlotElementFade).SetEase(Ease.InCirc);
            }
            else
            {
                this.colorImg.transform.localScale = Vector3.zero;
            }
            return;
        }

        this.colorImg.transform.DOKill();
        this.count.text = eb.amount.ToString();
        this.colorImg.color = ColoredMaterials.Instance.GetColorByColorIndex(eb.color);
        if (animate)
        {
            this.colorImg.transform.DOScale(Vector3.one, Durations.SlotElementFade).SetEase(Ease.OutBack);
        }
        else
        {
            this.colorImg.transform.localScale = Vector3.one;
        }
    }
}