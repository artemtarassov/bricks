using TMPro;
using UnityEngine;

public class AddSpaceView : DefaultView
{

    [SerializeField] private BtnAddSpaceAd btnAddSpaceAd;
    [SerializeField] private BtnIAP btnAddSpaceIAP;

    [SerializeField] private TMP_Text minutes;

    void Start()
    {
        Debug.Log("AddSpaceView: Start");
        this.btnAddSpaceAd.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnAddSpaceAdClicked);
        this.btnAddSpaceIAP.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnAddSpaceIAPClicked);
    }

    private void OnAddSpaceIAPClicked()
    {
        Debug.Log("AddSpaceView: OnAddSpaceIAPClicked");
        new HideViewCmd(ViewName.AddSpaceView).Run();
        new RequestPurchaseCmd(IAPModel.AdditionalSpace).Run();
    }

    private void OnAddSpaceAdClicked()
    {
        Debug.Log("AddSpaceView: OnAddSpaceAdClicked");
        new HideViewCmd(ViewName.AddSpaceView).Run();
        new ShowAdCmd().Run(RewardName.SPACE1);
    }
    public override void OnShown()
    {
        Debug.Log("AddSpaceView");
        var seconds = RemoteConfigModel.Instance.RemoteConfig.AdditionalEmitterSec;
        var min = Mathf.RoundToInt(seconds / 60.0f);
        this.minutes.text = min + " min";
    }

    public override void OnHidden()
    {

    }
    public override void OnBackgroundTap()
    {
        new HideViewCmd(ViewName.AddSpaceView).Run();
    }

}