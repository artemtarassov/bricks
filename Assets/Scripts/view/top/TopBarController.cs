using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backButton;

    private ViewModel viewModel => ViewModel.Instance;
    private PlayerModel playerModel => PlayerModel.Instance;

    private bool isInitialized;

    private void Start()
    {
        AssertDependencies();
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
    }

    private void OnBottomNavChanged(BottomNav _) => Refresh();

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
    }

    private void Unsubscribe()
    {
        this.viewModel.OnBottomNavChange -= OnBottomNavChanged;
        this.playerModel.OnPlayerDataChanged -= OnPlayerDataChanged;
        this.settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        this.backButton.onClick.RemoveListener(OnBackButtonClicked);
    }

    private void Refresh()
    {
        Assert.IsNotNull(this.playerModel.playerData, "TopBarController: PlayerModel.playerData is not initialized.");
        var bottomNav = this.viewModel.CurrentBottomNav;
        UpdateNavigationVisibility(bottomNav);
    }

    private void UpdateNavigationVisibility(BottomNav bottomNav)
    {
        this.backButton.gameObject.SetActive(bottomNav == BottomNav.FinishElement || bottomNav == BottomNav.Slots);

        /*var lastUnlockedChallenge = ChallengeModel.Instance.GetLastUnlockedChallenge();
        this.challengesButton.gameObject.SetActive(bottomNav == BottomNav.MainNav && lastUnlockedChallenge != BuildingName.Undefined);
        if (this.challengesButton.gameObject.activeSelf)
        {
            var icon = this.challengesButton.GetComponentInChildren<ChallengeIcon>();
            icon.Setup(lastUnlockedChallenge);
        }*/
    }

    private void AssertDependencies()
    {
        Assert.IsNotNull(this.settingsButton, "TopBarController: settingsButton is not assigned.");
        Assert.IsNotNull(this.backButton, "TopBarController: backButton is not assigned.");
        Assert.IsNotNull(ViewModel.Instance, "TopBarController: ViewModel.Instance is not initialized.");
        Assert.IsNotNull(PlayerModel.Instance, "TopBarController: PlayerModel.Instance is not initialized.");
        Assert.IsNotNull(PlayerModel.Instance.playerData, "TopBarController: PlayerModel.playerData is not initialized.");
    }
}
