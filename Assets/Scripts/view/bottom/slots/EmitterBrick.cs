using DG.Tweening;
using TMPro;
using UnityEngine;

public class EmitterBrick : MonoBehaviour
{
    private const float ContentRaisedY = 20f;
    private const float ContentMoveDuration = 0.3f;

    [HideInInspector]
    public BrickData brickData;

    [SerializeField] private TMP_Text count;
    [SerializeField] private TMP_Text timeout;
    [SerializeField] private UIBrick uiBrick;
    [SerializeField] private GameObject death;
    [SerializeField] private GameObject content;

    private Tween timeoutUpdateSequence;
    private int timeoutTimestamp;
    private bool moveContentUp;
    private bool isInitialized;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnDestroy()
    {
        StopTimeoutUpdates();
        content.transform.DOKill();
        uiBrick.transform.DOKill();
    }

    public void SetTimeout(int timeoutTimestamp)
    {
        EnsureInitialized();

        if (timeoutUpdateSequence == null)
        {
            timeoutUpdateSequence = DOTween.Sequence(this).AppendInterval(1)
                .AppendCallback(OnTimeoutUpdate)
                .SetLoops(-1);
        }

        this.timeoutTimestamp = timeoutTimestamp;
        OnTimeoutUpdate();
    }

    public void RemoveTimeout()
    {
        EnsureInitialized();

        StopTimeoutUpdates();
        timeout.gameObject.SetActive(false);
        timeoutTimestamp = 0;
        UpdateContentPosition();
    }

    public void Setup(Color color, EmitterSpace emitterSpace, bool animate = false)
    {
        EnsureInitialized();

        brickData = emitterSpace.brickData;
        //Debug.Log($"EmitterBrick: Setup called with brickData {(brickData != null ? brickData.color.ToString() : "null")} and isDead {emitterSpace.isDead}");

        if (emitterSpace.isDead)
        {
            ShowDeadEmitter(emitterSpace);
            return;
        }

        if (brickData == null)
        {
            ShowEmptyEmitter(animate);
            return;
        }

        ShowFilledEmitter(color, animate);
    }

    private void EnsureInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        //Debug.Log("EmitterBrick: initializing");

        isInitialized = true;
        timeoutUpdateSequence = null;
        timeoutTimestamp = 0;
        moveContentUp = false;

        content.transform.localPosition = Vector3.zero;
        timeout.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
        uiBrick.gameObject.SetActive(false);
        uiBrick.transform.localScale = Vector3.zero;
        death.SetActive(false);
    }

    private void StopTimeoutUpdates()
    {
        if (timeoutUpdateSequence == null)
        {
            return;
        }

        timeoutUpdateSequence.Kill();
        timeoutUpdateSequence = null;
    }

    private void OnTimeoutUpdate()
    {
        var currentTimestamp = TimeUtils.GetUnixTimestamp();
        var remaining = timeoutTimestamp - currentTimestamp;
        if (remaining < 0)
        {
            RemoveTimeout();
            return;
        }

        var minutesLeft = remaining / 60;
        if (minutesLeft >= 1)
        {
            timeout.text = "- " + minutesLeft + "m -";
        }
        else
        {
            timeout.text = TimeUtils.GetTimeLeft(remaining, "en");
        }

        if (!count.gameObject.activeSelf)
        {
            timeout.gameObject.SetActive(true);
        }

        UpdateContentPosition();
    }

    private void ShowEmptyEmitter(bool animate)
    {
        var wasOpen = count.gameObject.activeSelf;
        if (wasOpen)
        {
            new SoundCmd(SoundModel.Instance.EMITTER_CLOSE).Run();
        }

        count.gameObject.SetActive(false);
        uiBrick.gameObject.SetActive(false);
        death.SetActive(false);

        if (animate)
        {
            uiBrick.transform.DOKill();
            uiBrick.transform.DOScale(Vector3.zero, Durations.SlotElementFade).SetEase(Ease.InCirc);
        }
        else
        {
            uiBrick.transform.localScale = Vector3.zero;
        }

        UpdateContentPosition();
    }

    private void ShowDeadEmitter(EmitterSpace emitterSpace)
    {
        uiBrick.transform.DOKill();
        uiBrick.gameObject.SetActive(false);
        uiBrick.transform.localScale = Vector3.zero;

        count.gameObject.SetActive(true);
        count.text = emitterSpace.deadCounter.ToString();
        death.SetActive(true);

        UpdateContentPosition();
    }

    private void ShowFilledEmitter(Color color, bool animate)
    {
        var wasClosed = !count.gameObject.activeSelf;
        if (wasClosed)
        {
            new SoundCmd(SoundModel.Instance.EMITTER_OPEN).Run();
        }

        death.SetActive(false);
        count.text = brickData.coloredAmount.ToString();
        count.gameObject.SetActive(true);
        uiBrick.gameObject.SetActive(true);
        uiBrick.SetColor(color, brickData.color);
        uiBrick.ShowGloss(true);

        uiBrick.transform.DOKill();
        if (animate)
        {
            uiBrick.transform.DOScale(Vector3.one, Durations.SlotElementFade).SetEase(Ease.OutBack);
        }
        else
        {
            uiBrick.transform.localScale = Vector3.one;
        }

        UpdateContentPosition();
    }

    private void UpdateContentPosition()
    {
        var shouldRaiseContent = timeout.gameObject.activeSelf || count.gameObject.activeSelf;
        if (timeout.gameObject.activeSelf && count.gameObject.activeSelf)
        {
            timeout.gameObject.SetActive(false);
            shouldRaiseContent = true;
        }

        if (shouldRaiseContent)
        {
            MoveContentUp();
            return;
        }

        MoveContentDown();
    }

    private void MoveContentUp()
    {
        if (moveContentUp)
        {
            return;
        }

        moveContentUp = true;
        content.transform.DOKill();
        content.transform.DOLocalMoveY(ContentRaisedY, ContentMoveDuration).SetEase(Ease.OutQuad);
    }

    private void MoveContentDown()
    {
        if (!moveContentUp)
        {
            return;
        }

        moveContentUp = false;
        content.transform.DOKill();
        content.transform.DOLocalMoveY(0f, ContentMoveDuration).SetEase(Ease.OutBack);
    }
}
