using UnityEngine;

public class OutOfSpaceView : DefaultView
{
    [SerializeField] private BtnFreeAttempt btnFreeAttempt;
    [SerializeField] private BtnRefill btnRefillAttempts;
    [SerializeField] private BtnContinue btnContinue;
    [SerializeField] private BtnRestart btnRestart;
    [SerializeField] private AttemptsRow attemptsRow;

    [SerializeField] private GameObject tapToClose;

    void Start()
    {
        this.btnFreeAttempt.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnFreeAttemptClicked);
        this.btnRefillAttempts.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnRefillAttemptsClicked);
        this.btnContinue.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnContinueClicked);
        this.btnRestart.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnRestartClicked);
    }

    private void OnRestartClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.Restart);
        new HideViewCmd().Run(ViewName.OutOfSpaceView);
    }

    private void UpdateValues()
    {
        var a = PlayerModel.Instance.playerData.attempts;
        this.attemptsRow.UpdateValues(a);
        this.btnContinue.gameObject.SetActive(a > 0);
        this.btnRestart.gameObject.SetActive(a == 0);
        this.tapToClose.gameObject.SetActive(a > 0);
    }

    private void OnFreeAttemptClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.FreeAttemptForAd);
    }

    private void OnContinueClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.ContinueNextAttempt);
        new HideViewCmd().Run(ViewName.OutOfSpaceView);
    }

    private void OnRefillAttemptsClicked()
    {
        new BtnCmd().Run(BtnCmd.BtnAction.RefillAttempts);
    }


    public override void OnShown()
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
        UpdateValues();
    }

    public override void OnBackgroundTap()
    {
        var a = PlayerModel.Instance.playerData.attempts;
        var isGameOver = a == 0;
        if (!isGameOver)
            new HideViewCmd().Run(ViewName.OutOfSpaceView);
    }
}