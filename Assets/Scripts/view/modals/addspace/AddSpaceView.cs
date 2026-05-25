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
    }

    private void OnAddSpaceAdClicked()
    {
        Debug.Log("AddSpaceView: OnAddSpaceAdClicked");
        new ShowAdCmd().Run(RewardName.SPACE1);
    }
    public override void OnShown(bool animate)
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