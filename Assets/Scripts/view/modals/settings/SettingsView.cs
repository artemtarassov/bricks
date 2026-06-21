using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SettingsView : DefaultView
{
    [SerializeField] private Button restoreBtn;
    [SerializeField] private Button rateBtn;
    [SerializeField] private Button goldenTicketBtn;
    void Start()
    {
        this.restoreBtn.onClick.AddListener(OnRestoreBtnClick);
        this.rateBtn.onClick.AddListener(OnRateBtnClick);
        this.goldenTicketBtn.onClick.AddListener(OnGoldenTicketBtnClick);

#if UNITY_ANDROID
        restoreBtn.gameObject.SetActive(false);
#endif
    }
    void OnDestroy()
    {
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
    }

    private void OnGoldenTicketBtnClick()
    {
        new ShowViewCmd(ViewName.GoldenTicketView).Run();
    }

    public override void OnBackgroundTap()
    {
        new HideViewCmd(ViewName.SettingsView).Run();
    }

    private void OnRestoreBtnClick()
    {
        new RestorePurchasesCmd().Run();
    }

    private void OnRateBtnClick()
    {
        new RateAppCmd().Run();
    }


    public override void OnHidden()
    {
        Debug.Log("DefaultView OnHidden called");
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
    }

    public override void OnShown()
    {
        Debug.Log("DefaultView OnShown called");
        PlayerModel.Instance.OnPlayerDataChanged += OnPlayerDataChanged;

        var hasGoldenTicket = IAPModel.Instance.DidPurchaseComplete(IAPProductName.GoldenTicket) || IAPModel.Instance.HasTempGoldenTicket();
        this.goldenTicketBtn.gameObject.SetActive(!hasGoldenTicket);
    }

    private void OnPlayerDataChanged()
    {
        RefreshAllCheckmarks();
    }

    private void RefreshAllCheckmarks()
    {
        var checkmarkRows = GetComponentsInChildren<SettingsCheckmarkRow>(true);
        foreach (var row in checkmarkRows)
        {
            row.Refresh();
        }
    }

}