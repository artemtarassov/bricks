using TMPro;
using UnityEngine;

public class GameOverView : DefaultView
{
    [SerializeField] private BtnFreeAttempt btnFreeAttempt;
    [SerializeField] private BtnRefill btnRefillAttempts;
    [SerializeField] private BtnContinue btnContinue;
    [SerializeField] private BtnRestart btnRestart;
    [SerializeField] private AttemptsRow attemptsRow;

    [SerializeField] private GameObject tapToClose;

    [SerializeField] private TMP_Text title;

    void Start()
    {
        this.btnFreeAttempt.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnFreeAttemptClicked);
        this.btnRefillAttempts.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnRefillAttemptsClicked);
        this.btnContinue.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnContinueClicked);
        this.btnRestart.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnRestartClicked);
    }

    private BuildingName buildingName => PlayerModel.Instance.playerData.CurrentBuildingName;

    private void OnRestartClicked()
    {
        new HideViewCmd(ViewName.GameOverView).Run();
        new BtnCmd(buildingName).Run(BtnCmd.BtnAction.Restart);
    }

    private void UpdateValues()
    {
        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        var a = progress.attempts;
        this.attemptsRow.UpdateValues(a);
        this.btnContinue.gameObject.SetActive(a > 0);
        this.btnRestart.gameObject.SetActive(a == 0);
        this.tapToClose.gameObject.SetActive(a > 0);


        var reason = ModelUtils.IsGameOver();
        if (reason == GameOverReason.OutOfSpace)
        {
            this.title.text = "Out of space";
            return;
        }

        if (reason == GameOverReason.OutOfTime)
        {
            this.title.text = "Out of time";
            return;
        }

    }

    private void OnFreeAttemptClicked()
    {
        new BtnCmd(buildingName).Run(BtnCmd.BtnAction.FreeAttemptForAd);
    }

    private void OnContinueClicked()
    {
        new HideViewCmd(ViewName.GameOverView).Run();
        new BtnCmd(buildingName).Run(BtnCmd.BtnAction.UseAttemptAndContinue);
    }

    private void OnRefillAttemptsClicked()
    {
        new BtnCmd(buildingName).Run(BtnCmd.BtnAction.RefillAttempts);
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
        new HideViewCmd(ViewName.GameOverView).Run();
    }
}