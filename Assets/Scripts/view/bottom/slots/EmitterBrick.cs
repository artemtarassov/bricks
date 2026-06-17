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
    [SerializeField] private UIBrick uiBrick;
    [SerializeField] private GameObject death;
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
        this.death.SetActive(false);
        this.uiBrick.transform.localScale = Vector3.zero;
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

    public void Setup(Color clr, EmitterSpace eb, bool animate = false)
    {
        this.brickData = eb.brickData;

        if (this.brickData == null)
        {
            var wasOpen = this.count.gameObject.activeSelf == true;
            if (wasOpen)
                new SoundCmd(SoundModel.Instance.EMITTER_CLOSE).Run();
            this.count.gameObject.SetActive(false);
            this.uiBrick.gameObject.SetActive(false);
            this.death.SetActive(eb.isDead);
            if (animate)
            {
                this.isAnimating = true;
                this.uiBrick.transform.DOScale(Vector3.zero, Durations.SlotElementFade).SetEase(Ease.InCirc).OnComplete(() => this.isAnimating = false);
            }
            else
            {
                this.uiBrick.transform.localScale = Vector3.zero;
            }
            this.UpdateContentPos();
            return;
        }

        var wasClosed = this.count.gameObject.activeSelf == false;
        if (wasClosed)
        {
            new SoundCmd(SoundModel.Instance.EMITTER_OPEN).Run();
        }
        else
        {
            //new SoundCmd(SoundModel.Instance.CLICK).Run();
        }

        if (eb.isDead)
        {
            this.isAnimating = false;
            this.uiBrick.transform.DOKill();
            this.count.gameObject.SetActive(false);
            this.uiBrick.ShowGloss(false);
            this.death.SetActive(true);
            this.UpdateContentPos();
            return;
        }


        this.count.text = brickData.coloredAmount.ToString();
        this.count.gameObject.SetActive(true);
        this.uiBrick.gameObject.SetActive(true);
        this.uiBrick.SetColor(clr, brickData.color);
        this.uiBrick.ShowGloss(true);
        if (animate)
        {
            //this.uiBrick.transform.localScale = Vector3.zero;
            this.isAnimating = true;
            this.uiBrick.transform.DOKill();
            this.uiBrick.transform.DOScale(Vector3.one, Durations.SlotElementFade).SetEase(Ease.OutBack).OnComplete(() => this.isAnimating = false);
        }
        else
        {
            this.uiBrick.transform.localScale = Vector3.one;
        }
        this.UpdateContentPos();
    }
}