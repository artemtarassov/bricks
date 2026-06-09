using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Transform coinsIcon;

    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backButton;
    private int prevCoinsAmount = 0;
    private int fromAmount = 0;



    void Start()
    {
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
        new GoBackBtnCmd().Run();
    }

    void OnDestroy()
    {
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
    }


    private void OnPlayerDataChanged()
    {
        var state = PlayerModel.Instance.playerData.GetCurrentGroupProgress().state;
        this.backButton.gameObject.SetActive(state == GroupState.Playing);

        
        var n = PlayerModel.Instance.playerData.coins;
        if (prevCoinsAmount == n)
        {
            return;
        }
        if (prevCoinsAmount > n)
        {
            this.coinsText.DOKill();
            this.coinsIcon.DOKill();
            this.coinsIcon.localScale = Vector3.one;
            this.coinsText.text = n.ToString("N0");
            this.prevCoinsAmount = n;
            return;
        }
        this.coinsIcon.DOKill();
        this.coinsIcon.localScale = Vector3.one;
        this.coinsIcon.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f).SetEase(Ease.OutCubic);
        this.coinsText.DOKill();

        this.fromAmount = prevCoinsAmount;
        this.prevCoinsAmount = n;

        DOTween.To(() => fromAmount, x => coinsText.text = x.ToString("N0"), n, 0.1f).SetEase(Ease.Linear).SetTarget(this.coinsText);

    }
}