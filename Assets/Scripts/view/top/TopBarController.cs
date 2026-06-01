using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBarController : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;

    [SerializeField] private Button settingsButton;

    void Start()
    {
        OnPlayerDataChanged();
        PlayerModel.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
        this.settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }

    private void OnSettingsButtonClicked()
    {
        new ShowViewCmd().Run(ViewName.SettingsView);
    }

    void OnDestroy()
    {
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
    }

    private void OnPlayerDataChanged()
    {
        var n = PlayerModel.Instance.playerData.coins;
        this.coinsText.text = n.ToString("N0");
    }
}