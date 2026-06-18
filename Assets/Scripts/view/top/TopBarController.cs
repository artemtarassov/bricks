using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Transform coinsIcon;
    [SerializeField] private GameObject coinsObject;

    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backButton;
    private int prevCoinsAmount = 0;
    private int fromAmount = 0;



    void Start()
    {
        this.coinsObject.SetActive(false);
        UpdateVisibility();
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChanged;
        PlayerModel.Instance.OnPlayerDataChanged += UpdateVisibility;
        this.settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        this.backButton.onClick.AddListener(OnBackButtonClicked);

    }

    private void OnBottomNavChanged(BottomNav _)
    {
        UpdateVisibility();
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

    void OnDestroy()
    {
        ViewModel.Instance.OnBottomNavChange -= OnBottomNavChanged;
        this.settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        this.backButton.onClick.RemoveListener(OnBackButtonClicked);
        PlayerModel.Instance.OnPlayerDataChanged -= UpdateVisibility;
    }


    private void UpdateVisibility()
    {
        var pd = PlayerModel.Instance.playerData;
        var coins = pd.coins;
        var bottomNav = ViewModel.Instance.CurrentBottomNav;

        this.backButton.gameObject.SetActive(bottomNav != BottomNav.MainNav);
        this.coinsObject.SetActive(coins > 0 && bottomNav != BottomNav.MainNav);


        if (prevCoinsAmount == coins)
        {
            return;
        }


        if (prevCoinsAmount > coins)
        {
            this.coinsText.DOKill();
            this.coinsIcon.DOKill();
            this.coinsIcon.localScale = Vector3.one;
            this.coinsText.text = coins.ToString("N0");
            this.prevCoinsAmount = coins;
            return;
        }
        this.coinsIcon.DOKill();
        this.coinsIcon.localScale = Vector3.one;
        this.coinsIcon.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f).SetEase(Ease.OutCubic);
        this.coinsText.DOKill();

        this.fromAmount = prevCoinsAmount;
        this.prevCoinsAmount = coins;

        DOTween.To(() => fromAmount, x => coinsText.text = x.ToString("N0"), coins, 0.1f).SetEase(Ease.Linear).SetTarget(this.coinsText);

    }
}