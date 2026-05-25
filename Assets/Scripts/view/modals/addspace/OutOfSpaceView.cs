using UnityEngine;

public class OutOfSpaceView : DefaultView
{
    [SerializeField] private BtnFreeAttempt btnFreeAttempt;
    [SerializeField] private BtnRefill btnRefillAttempts;
    [SerializeField] private BtnContinue btnContinue;
    [SerializeField] private AttemptsRow attemptsRow;

    void Start()
    {
        this.btnFreeAttempt.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnFreeAttemptClicked);
        this.btnRefillAttempts.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnRefillAttemptsClicked);
        this.btnContinue.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnContinueClicked);
    }

    private void OnFreeAttemptClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.FreeAttemptForAd);
    }

    private void OnContinueClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.ContinueNextAttempt);
    }

    private void OnRefillAttemptsClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.RefillAttempts);
    }


    public override void OnShown(bool animate)
    {
        PlayerModel.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
        OnPlayerDataChanged();
    }

    public override void OnHidden()
    {
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
    }

    private void OnPlayerDataChanged()
    {
        this.attemptsRow.UpdateValues(PlayerModel.Instance.playerData.attempts);
    }

    public override void OnBackgroundTap()
    {
        new HideViewCmd().Run(ViewName.OutOfSpaceView);
    }
}