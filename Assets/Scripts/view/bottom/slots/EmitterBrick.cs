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
    [SerializeField] private GameObject content;


    private Tween timeoutUpdateSequence;

    private bool isAnimating = false;

    private int timeoutTimestamp;

    void Awake()
    {
        this.content.transform.localPosition = Vector3.zero;
        this.timeoutUpdateSequence = null;
        this.timeoutTimestamp = 0;
        this.timeout.gameObject.SetActive(false);
        this.count.gameObject.SetActive(false);
        this.colorImg.transform.localScale = Vector3.zero;
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
        this.UpdateContentPos();
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
        if (this.count.gameObject.activeSelf == false)
            this.timeout.gameObject.SetActive(true);
        this.UpdateContentPos();
    }

    private bool moveContentUp = false;
    private void UpdateContentPos()
    {
        if (this.timeout.gameObject.activeSelf || this.count.gameObject.activeSelf)
        {
            if (this.timeout.gameObject.activeSelf && this.count.gameObject.activeSelf)
            {
                this.timeout.gameObject.SetActive(false);
            }

            if (moveContentUp)
            {
                return;
            }
            moveContentUp = true;
            this.content.transform.DOKill();
            this.content.transform.DOLocalMoveY(20, 0.3f).SetEase(Ease.OutQuad);
        }
        else
        {
            if (!moveContentUp)
            {
                return;
            }
            moveContentUp = false;
            this.content.transform.DOKill();
            this.content.transform.DOLocalMoveY(0, 0.3f).SetEase(Ease.OutBack);
        }
    }

    public void Setup(BrickData eb = null, bool animate = false)
    {
        this.brickData = eb;

        if (eb == null)
        {
            var wasOpen = this.count.gameObject.activeSelf == true;
            if (wasOpen)
                new SoundCmd(SoundModel.Instance.EMITTER_CLOSE).Run();
            this.count.gameObject.SetActive(false);
            this.colorImg.transform.DOKill();
            if (animate)
            {
                this.isAnimating = true;
                this.colorImg.transform.DOScale(Vector3.zero, Durations.SlotElementFade).SetEase(Ease.InCirc).OnComplete(() => this.isAnimating = false);
            }
            else
            {
                this.colorImg.transform.localScale = Vector3.zero;
            }
            this.UpdateContentPos();
            return;
        }

        var wasClosed = this.count.gameObject.activeSelf == false;
        if (wasClosed)
        {
            new SoundCmd(SoundModel.Instance.EMITTER_OPEN).Run();
        } else
        {
            //new SoundCmd(SoundModel.Instance.CLICK).Run();
        }


        this.count.text = eb.coloredAmount.ToString();
        this.count.gameObject.SetActive(true);
        this.colorImg.color = Color.white;
        this.colorImg.sprite = ColoredMaterials.Instance.GetSpriteByColorIndex(eb.color);
        if (animate)
        {
            //this.colorImg.transform.localScale = Vector3.zero;
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
            this.colorImg.transform.localScale = Vector3.one;
        }
        this.UpdateContentPos();
    }
}