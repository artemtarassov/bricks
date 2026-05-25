using TMPro;
using UnityEngine;

public class TopBarController : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;
    void Start()
    {
        OnPlayerDataChanged();
        PlayerModel.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
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