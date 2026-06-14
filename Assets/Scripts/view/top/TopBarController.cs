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
        OnPlayerDataChanged();
        PlayerModel.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
        this.settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        this.backButton.onClick.AddListener(OnBackButtonClicked);

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
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
    }


    private void OnPlayerDataChanged()
    {
        var pd = PlayerModel.Instance.playerData;
        var coins = pd.coins;
        var state = pd.GetCurrentBuildingProgress().State;

        this.backButton.gameObject.SetActive(state == BuildingState.Playing);
        this.coinsObject.SetActive(coins > 0 || state == BuildingState.Playing);


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