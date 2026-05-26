using UnityEngine;

public class AddSpaceView : DefaultView
{

    [SerializeField] private BtnAddSpaceAd btnAddSpaceAd;
    [SerializeField] private BtnAddSpaceIAP btnAddSpaceIAP;

    void Start()
    {
        Debug.Log("AddSpaceView: Start");
        this.btnAddSpaceAd.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnAddSpaceAdClicked);
        this.btnAddSpaceIAP.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnAddSpaceIAPClicked);
    }

    private void OnAddSpaceIAPClicked()
    {
        Debug.Log("AddSpaceView: OnAddSpaceIAPClicked");
        new HideViewCmd().Run(ViewName.AddSpaceView);
        new RequestPurchaseCmd(IAPModel.AdditionalSpace).Run();
    }

    private void OnAddSpaceAdClicked()
    {
        Debug.Log("AddSpaceView: OnAddSpaceAdClicked");
        new HideViewCmd().Run(ViewName.AddSpaceView);
        new ShowAdCmd().Run(RewardName.SPACE1);
    }
    public override void OnShown()
    {
        Debug.Log("AddSpaceView");
    }

    public override void OnHidden()
    {

    }
    public override void OnBackgroundTap()
    {
        new HideViewCmd().Run(ViewName.AddSpaceView);
    }

}