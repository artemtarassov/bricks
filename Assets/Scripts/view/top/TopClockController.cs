using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class TopClockController : MonoBehaviour
{
    private const float ClockPunchScale = 0.2f;
    private const float ClockPunchDuration = 0.2f;
    private const int ClockPunchVibrato = 1;
    private const float ClockPunchElasticity = 0.5f;
    private const float ClockCountAnimationDuration = 0.1f;

    [SerializeField] private TMP_Text ClockText;
    [SerializeField] private Transform ClockIcon;
    [SerializeField] private GameObject ClockObject;

    private ViewModel viewModel => ViewModel.Instance;
    private PlayerModel playerModel => PlayerModel.Instance;
    private RectTransform rootRectTransform;
    private RectTransform ClockObjectRectTransform;
    private int displayedSecondsAmount;
    private bool isClockInitialized;
    private bool isInitialized;

    private Tween secUpdateTween;

    private void Start()
    {
        AssertDependencies();
        this.rootRectTransform = this.gameObject.GetComponent<RectTransform>();
        this.ClockObjectRectTransform = this.ClockObject.GetComponent<RectTransform>();
        this.ClockObject.SetActive(false);

        Subscribe();
        this.isInitialized = true;
    }

    private void OnDestroy()
    {
        if (!this.isInitialized)
        {
            return;
        }

        Unsubscribe();
        KillSecondsTweens();
        if (this.secUpdateTween != null)
        {
            this.secUpdateTween.Kill();
            this.secUpdateTween = null;
        }

    }

    private void OnTopNavChanged(TopNav topNav)
    {
        if (topNav == TopNav.Clock)
        {
            this.ClockText.text = "-";
            this.ClockObject.SetActive(true);
            ViewModel.Instance.OnClockTimeIncreased -= OnClockTimeIncreased;
            ViewModel.Instance.OnClockTimeIncreased += OnClockTimeIncreased;
            OnClockTimeIncreased();
        }
        else
        {
            if (this.secUpdateTween != null)
            {
                this.secUpdateTween.Kill();
                this.secUpdateTween = null;
            }
            ViewModel.Instance.OnClockTimeIncreased -= OnClockTimeIncreased;

            this.ClockObject.SetActive(false);
        }
    }


    private void OnSec()
    {
        Refresh();
    }

    private void OnClockTimeIncreased()
    {
        if (this.secUpdateTween != null)
        {
            this.secUpdateTween.Kill();
            this.secUpdateTween = null;
        }
        UpdateClockDisplay(GetSecondsLeft(), true);
        this.secUpdateTween = DOTween.Sequence(this).AppendInterval(1f).AppendCallback(OnSec).SetLoops(-1);
    }

    private void Subscribe()
    {
        this.viewModel.OnTopNavChange += OnTopNavChanged;
    }

    private void Unsubscribe()
    {
        this.viewModel.OnTopNavChange -= OnTopNavChanged;
    }

    private int GetSecondsLeft()
    {
        var progress = this.playerModel.playerData.GetCurrentBuildingProgress();
        var element = progress.GetCurrentElement();
        Assert.IsNotNull(element, "TopClockController: Current element is null. Cannot refresh clock display.");
        var seconds = element.timeoutSeconds;
        return seconds;
    }

    private void Refresh(bool forceImmediate = false)
    {
        UpdateClockDisplay(GetSecondsLeft(), false);
    }

    private void AssertDependencies()
    {
        Assert.IsNotNull(this.ClockText, "TopClockController: ClockText is not assigned.");
        Assert.IsNotNull(this.ClockIcon, "TopClockController: ClockIcon is not assigned.");
        Assert.IsNotNull(this.ClockObject, "TopClockController: ClockObject is not assigned.");
    }

    private void UpdateClockDisplay(int seconds, bool animate)
    {
        if (animate == false)
        {
            SetClockImmediately(seconds);
            return;
        }
        SetClockImmediately(seconds);
        AnimateClockIncrease(seconds);
    }

    private void SetClockImmediately(int seconds)
    {
        KillSecondsTweens();
        ResetClockIconScale();
        SetClockText(seconds);
    }

    private void AnimateClockIncrease(int seconds)
    {
        var tweenedClock = this.displayedSecondsAmount;

        KillSecondsTweens();
        ResetClockIconScale();

        this.ClockIcon
            .DOPunchScale(Vector3.one * ClockPunchScale, ClockPunchDuration, ClockPunchVibrato, ClockPunchElasticity)
            .SetEase(Ease.OutCubic)
            .SetTarget(this.ClockIcon);

        this.ClockText.transform.DOPunchScale(Vector3.one * ClockPunchScale, ClockPunchDuration, ClockPunchVibrato, ClockPunchElasticity)
            .SetEase(Ease.OutCubic)
            .SetTarget(this.ClockText.transform);
    }

    private void SetClockText(int seconds)
    {
        this.displayedSecondsAmount = seconds;

        if (seconds > 0)
            this.ClockText.text = TimeUtils.GetTimeLeft(seconds, "en");
        else
            this.ClockText.text = "-";
    }

    private void KillSecondsTweens()
    {
        this.ClockText.DOKill();
        this.ClockIcon.DOKill();
    }

    private void ResetClockIconScale()
    {
        this.ClockIcon.localScale = Vector3.one;
    }

    private void ForceLayoutRefresh()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.ClockText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.ClockObjectRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.rootRectTransform);
        Canvas.ForceUpdateCanvases();
    }
}
