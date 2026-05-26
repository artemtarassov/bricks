using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmitterBrick : MonoBehaviour
{
    [HideInInspector]
    public BrickData brickData;

    [SerializeField] private TMP_Text count;
    [SerializeField] private TMP_Text timeout;
    [SerializeField] private Image colorImg;


    private Tween timeoutUpdateSequence;

    private bool isAnimating = false;

    private int timeoutTimestamp;

    void Awake()
    {
        this.timeoutUpdateSequence = null;
        this.timeoutTimestamp = 0;
        this.timeout.gameObject.SetActive(false);
        this.colorImg.transform.localScale = Vector3.zero;
        this.count.text = "";
    }

    public void SetTimeout(int timeoutTimestamp)
    {
        if (this.timeoutUpdateSequence == null)
        {
            this.timeoutUpdateSequence = DOTween.Sequence(this).AppendInterval(1)
                .AppendCallback(OnTimeoutUpdate)
                .SetLoops(-1);
        }
        this.timeoutTimestamp = timeoutTimestamp;
        this.OnTimeoutUpdate();
    }

    public void RemoveTimeout()
    {
        if (this.timeoutUpdateSequence != null)
        {
            this.timeoutUpdateSequence.Kill();
            this.timeoutUpdateSequence = null;
        }
        this.timeout.gameObject.SetActive(false);
        this.timeoutTimestamp = 0;
    }

    private void OnTimeoutUpdate()
    {
        var currentTimestamp = TimeUtils.GetUnixTimestamp();
        var remaining = this.timeoutTimestamp - currentTimestamp;
        if (remaining < 0)
        {
            this.RemoveTimeout();
            return;
        }
        int minutesLeft = remaining / 60;
        if (minutesLeft >= 1)
        {
            this.timeout.text = "- " + minutesLeft + "m -";
        }
        else
        {
            this.timeout.text = TimeUtils.GetTimeLeft(remaining, "en");
        }
        this.timeout.gameObject.SetActive(true);
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


        this.count.text = eb.coloredAmount.ToString();
        this.colorImg.color = ColoredMaterials.Instance.GetColorByColorIndex(eb.color);
        if (animate)
        {
            if (this.isAnimating)
            {
                return;
            }
            this.colorImg.transform.localScale = Vector3.zero;
            this.isAnimating = true;
            this.colorImg.transform.DOKill();
            this.colorImg.transform.DOScale(Vector3.one, Durations.SlotElementFade).SetEase(Ease.OutBack).OnComplete(() => this.isAnimating = false);
        }
        else
        {
            if (!this.isAnimating)
            {
                //this.colorImg.transform.localScale = Vector3.one;
            }

        }
    }
}