using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    private const float CoinsPunchScale = 0.1f;
    private const float CoinsPunchDuration = 0.1f;
    private const int CoinsPunchVibrato = 1;
    private const float CoinsPunchElasticity = 0.5f;
    private const float CoinsCountAnimationDuration = 0.1f;

    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Transform coinsIcon;
    [SerializeField] private GameObject coinsObject;//Horizontal layout group containing coins icon and text, used to hide/show them together
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backButton;

    [SerializeField] private Button challengesButton;

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
        this.rootRectTransform = (RectTransform)transform;
        this.coinsObjectRectTransform = this.coinsObject.GetComponent<RectTransform>();

        this.coinsObject.SetActive(false);
        this.backButton.gameObject.SetActive(false);

        Subscribe();
        this.isInitialized = true;
        Refresh();
    }

    private void OnChallengesButtonClicked()
    {
        new ShowViewCmd(ViewName.ChallengesView).Run();
    }

    private void OnDestroy()
    {
        if (!this.isInitialized)
        {
            return;
        }

        Unsubscribe();
        KillCoinTweens();
    }

    private void OnBottomNavChanged(BottomNav _)
    {
        Refresh(true);
    }

    private void OnPlayerDataChanged()
    {
        Refresh();
    }

    private void OnSettingsButtonClicked()
    {
        new ShowViewCmd(ViewName.SettingsView).Run();
    }

    private void OnBackButtonClicked()
    {
        new SoundCmd(SoundModel.Instance.CLICK2).Run();
        new GoBackBtnCmd().Run();
    }

    private void Subscribe()
    {
        this.viewModel.OnBottomNavChange += OnBottomNavChanged;
        this.playerModel.OnPlayerDataChanged += OnPlayerDataChanged;
        this.settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        this.backButton.onClick.AddListener(OnBackButtonClicked);
        this.challengesButton.onClick.AddListener(OnChallengesButtonClicked);
    }

    private void Unsubscribe()
    {
        this.viewModel.OnBottomNavChange -= OnBottomNavChanged;
        this.playerModel.OnPlayerDataChanged -= OnPlayerDataChanged;
        this.settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        this.backButton.onClick.RemoveListener(OnBackButtonClicked);
        this.challengesButton.onClick.RemoveListener(OnChallengesButtonClicked);
    }

    private void Refresh(bool forceCoinsSync = false)
    {
        Assert.IsNotNull(this.playerModel.playerData, "TopBarController: PlayerModel.playerData is not initialized.");

        var coins = this.playerModel.playerData.coins;
        var bottomNav = this.viewModel.CurrentBottomNav;

        UpdateNavigationVisibility(bottomNav, coins);
        UpdateCoinsDisplay(coins, forceCoinsSync || !this.isCoinsInitialized);
        if (forceCoinsSync || !this.isCoinsInitialized)
        {
            ForceLayoutRefresh();
        }
        this.isCoinsInitialized = true;
    }

    private void UpdateNavigationVisibility(BottomNav bottomNav, int coins)
    {
        
        this.backButton.gameObject.SetActive(bottomNav == BottomNav.FinishElement || bottomNav == BottomNav.Slots);
        this.coinsObject.SetActive(coins > 0 || bottomNav == BottomNav.Slots);

        var lastUnlockedChallenge = ChallengeModel.Instance.GetLastUnlockedChallenge();
        //Debug.Log("lastUnlockedChallenge " + lastUnlockedChallenge);
        this.challengesButton.gameObject.SetActive(bottomNav == BottomNav.MainNav && lastUnlockedChallenge != BuildingName.Undefined);
        if (this.challengesButton.gameObject.activeSelf)
        {
            var icon = this.challengesButton.GetComponentInChildren<ChallengeIcon>();
            icon.Setup(lastUnlockedChallenge);
        }
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
        var startCoins = this.displayedCoinsAmount;
        var tweenedCoins = startCoins;

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
            .OnComplete(() => { SetCoinsText(coins); ForceLayoutRefresh(); });
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

    private void AssertDependencies()
    {
        Assert.IsNotNull(this.coinsText, "TopBarController: coinsText is not assigned.");
        Assert.IsNotNull(this.coinsIcon, "TopBarController: coinsIcon is not assigned.");
        Assert.IsNotNull(this.coinsObject, "TopBarController: coinsObject is not assigned.");
        Assert.IsNotNull(this.settingsButton, "TopBarController: settingsButton is not assigned.");
        Assert.IsNotNull(this.backButton, "TopBarController: backButton is not assigned.");
        Assert.IsNotNull(ViewModel.Instance, "TopBarController: ViewModel.Instance is not initialized.");
        Assert.IsNotNull(PlayerModel.Instance, "TopBarController: PlayerModel.Instance is not initialized.");
        Assert.IsNotNull(PlayerModel.Instance.playerData, "TopBarController: PlayerModel.playerData is not initialized.");
        Assert.IsNotNull(transform as RectTransform, "TopBarController: expected to be attached to a RectTransform.");
        Assert.IsNotNull(this.coinsObject.GetComponent<RectTransform>(), "TopBarController: coinsObject is expected to have a RectTransform.");
    }
}
