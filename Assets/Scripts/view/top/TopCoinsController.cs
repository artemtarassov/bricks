using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class TopCoinsController : MonoBehaviour
{
    private const float CoinsPunchScale = 0.1f;
    private const float CoinsPunchDuration = 0.1f;
    private const int CoinsPunchVibrato = 1;
    private const float CoinsPunchElasticity = 0.5f;
    private const float CoinsCountAnimationDuration = 0.1f;

    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Transform coinsIcon;
    [SerializeField] private GameObject coinsObject;

    private ViewModel viewModel => ViewModel.Instance;
    private PlayerModel playerModel => PlayerModel.Instance;
    private RectTransform rootRectTransform;
    private RectTransform coinsObjectRectTransform;
    private int targetCoinsAmount;
    private int displayedCoinsAmount;
    private bool isCoinsInitialized;
    private bool isInitialized;

    private void Start()
    {
        AssertDependencies();
        this.rootRectTransform = this.gameObject.GetComponent<RectTransform>();
        this.coinsObjectRectTransform = this.coinsObject.GetComponent<RectTransform>();
        this.coinsObject.SetActive(false);

        Subscribe();
        this.isInitialized = true;
        Refresh(true);
    }

    private void OnDestroy()
    {
        if (!this.isInitialized)
        {
            return;
        }

        Unsubscribe();
        KillCoinTweens();
        this.playerModel.OnPlayerDataChanged -= OnPlayerDataChanged;
    }

    private void OnTopNavChange(TopNav nav)
    {
        this.playerModel.OnPlayerDataChanged -= OnPlayerDataChanged;

        if (nav == TopNav.Coins)
        {
            this.coinsObject.SetActive(true);
            this.playerModel.OnPlayerDataChanged += OnPlayerDataChanged;
            this.Refresh(true);
        }
        else
        {
            this.coinsObject.SetActive(false);
        }
    }

    private void OnPlayerDataChanged()
    {
        Refresh();
    }

    private void Subscribe()
    {
        this.viewModel.OnTopNavChange += OnTopNavChange;
    }

    private void Unsubscribe()
    {
        this.viewModel.OnTopNavChange -= OnTopNavChange;
    }

    private void Refresh(bool forceImmediate = false)
    {
        var coins = this.playerModel.playerData.coins;
        UpdateCoinsDisplay(coins, forceImmediate || !this.isCoinsInitialized);
        if (forceImmediate || !this.isCoinsInitialized)
        {
            ForceLayoutRefresh();
        }

        this.isCoinsInitialized = true;
    }

    private void AssertDependencies()
    {
        Assert.IsNotNull(this.coinsText, "TopCoinsController: coinsText is not assigned.");
        Assert.IsNotNull(this.coinsIcon, "TopCoinsController: coinsIcon is not assigned.");
        Assert.IsNotNull(this.coinsObject, "TopCoinsController: coinsObject is not assigned.");
    }

    private void UpdateCoinsDisplay(int coins, bool forceImmediate)
    {
        if (forceImmediate)
        {
            SetCoinsImmediately(coins);
            return;
        }

        if (this.targetCoinsAmount == coins)
        {
            return;
        }

        if (coins < this.targetCoinsAmount)
        {
            SetCoinsImmediately(coins);
            return;
        }

        AnimateCoinsIncrease(coins);
    }

    private void SetCoinsImmediately(int coins)
    {
        KillCoinTweens();
        ResetCoinsIconScale();
        this.targetCoinsAmount = coins;
        SetCoinsText(coins);
    }

    private void AnimateCoinsIncrease(int coins)
    {
        var tweenedCoins = this.displayedCoinsAmount;

        KillCoinTweens();
        ResetCoinsIconScale();

        this.coinsIcon
            .DOPunchScale(Vector3.one * CoinsPunchScale, CoinsPunchDuration, CoinsPunchVibrato, CoinsPunchElasticity)
            .SetEase(Ease.OutCubic)
            .SetTarget(this.coinsIcon);

        this.targetCoinsAmount = coins;

        DOTween.To(
                () => tweenedCoins,
                value =>
                {
                    tweenedCoins = value;
                    SetCoinsText(value);
                },
                coins,
                CoinsCountAnimationDuration
            )
            .SetEase(Ease.Linear)
            .SetTarget(this.coinsText)
            .OnComplete(() =>
            {
                SetCoinsText(coins);
                ForceLayoutRefresh();
            });
    }

    private void SetCoinsText(int coins)
    {
        this.displayedCoinsAmount = coins;
        this.coinsText.text = coins.ToString("N0");
    }

    private void KillCoinTweens()
    {
        this.coinsText.DOKill();
        this.coinsIcon.DOKill();
    }

    private void ResetCoinsIconScale()
    {
        this.coinsIcon.localScale = Vector3.one;
    }

    private void ForceLayoutRefresh()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.coinsText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.coinsObjectRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.rootRectTransform);
        Canvas.ForceUpdateCanvases();
    }
}
