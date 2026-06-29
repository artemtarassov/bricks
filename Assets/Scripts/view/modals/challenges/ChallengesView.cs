using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.UI;

public class ChallengesView : DefaultView
{
    [SerializeField] private BtnRestart startBtn;
    [SerializeField] private Button freeAttemptBtn;
    [SerializeField] private Button refillAttemptsBtn;

    [SerializeField] private AttemptsRow attemptsRow;

    private List<ChallengeButton> challengeButtons = new List<ChallengeButton>();

    private BuildingName selectedBuilding;

    void Awake()
    {
        this.challengeButtons.AddRange(this.GetComponentsInChildren<ChallengeButton>(true));
        foreach (var btn in this.challengeButtons)
        {
            btn.GetComponent<HoldButton>().OnClick.AddListener(() => OnClickChallengeButton(btn));
        }
        this.selectedBuilding = BuildingName.Undefined;
        this.startBtn.GetComponent<HoldButton>().OnClick.AddListener(OnClickStart);

        this.freeAttemptBtn.GetComponent<HoldButton>().OnClick.AddListener(OnFreeAttemptClicked);
        this.refillAttemptsBtn.GetComponent<HoldButton>().OnClick.AddListener(OnRefillAttemptsClicked);
    }

    private void OnClickChallengeButton(ChallengeButton btn)
    {
        new SoundCmd(SoundModel.Instance.CLICK1).Run();
        this.selectedBuilding = btn.challengeData.buildingName;
        foreach (var cb in challengeButtons)
        {
            cb.SetSelected(cb.challengeData.buildingName == this.selectedBuilding);
        }
        UpdateProgress();
        this.UpdateStartButton();
    }

    private void OnClickStart()
    {
        new HideViewCmd(ViewName.ChallengesView).Run();
        new SoundCmd(SoundModel.Instance.CLICK2).Run();
        new BtnCmd(this.selectedBuilding).Run(BtnCmd.BtnAction.UseAttemptAndContinue);
    }

    public override void OnBackgroundTap()
    {
        new HideViewCmd(ViewName.ChallengesView).Run();
    }

    private void OnFreeAttemptClicked()
    {
        new BtnCmd(this.selectedBuilding).Run(BtnCmd.BtnAction.FreeAttemptForAd);
    }

    private void OnRefillAttemptsClicked()
    {
        new BtnCmd(this.selectedBuilding).Run(BtnCmd.BtnAction.RefillAttempts);
    }


    void Start()
    {
    }
    void OnDestroy()
    {
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
    }

    public override void OnHidden()
    {
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
    }

    public override void OnShown()
    {
        if (this.selectedBuilding == BuildingName.Undefined)
        {
            var unlockedChallenge = ChallengeModel.Instance.GetAllChallenges().FindLast((e) => e.isLocked == false);
            this.selectedBuilding = unlockedChallenge.buildingName;
        }
        this.UpateButtons();
        this.UpdateProgress();
        this.UpdateStartButton();
        ViewModel.Instance.ChangeTopNav(TopNav.Coins);
        PlayerModel.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
    }

    private void OnPlayerDataChanged()
    {
        this.UpdateProgress();
        this.UpdateStartButton();
    }

    private void UpdateStartButton()
    {
        var challenge = ChallengeModel.Instance.GetAllChallenges().Find(c => c.buildingName == this.selectedBuilding);
        if (challenge.isLocked)
        {
            this.startBtn.GetComponent<Button>().interactable = false;
            this.freeAttemptBtn.gameObject.SetActive(false);
            this.refillAttemptsBtn.gameObject.SetActive(false);
        }
        else
        {
            if (attempts < 1)
            {
                this.freeAttemptBtn.gameObject.SetActive(true);
                this.refillAttemptsBtn.gameObject.SetActive(true);
                this.startBtn.gameObject.SetActive(false);
            }
            else
            {
                this.freeAttemptBtn.gameObject.SetActive(false);
                this.refillAttemptsBtn.gameObject.SetActive(false);
                this.startBtn.GetComponent<Button>().interactable = true;
                this.startBtn.gameObject.SetActive(true);
            }

        }
    }

    private int attempts => PlayerModel.Instance.playerData.GetBuildingProgressByName(this.selectedBuilding).attempts;

    private void UpdateProgress()
    {
        if (this.selectedBuilding == BuildingName.Undefined)
        {
            this.attemptsRow.gameObject.SetActive(false);
        }
        this.attemptsRow.UpdateValues(attempts);
        this.attemptsRow.gameObject.SetActive(true);
    }

    private void UpateButtons()
    {
        var challenges = ChallengeModel.Instance.GetAllChallenges();
        for (var i = 0; i < this.challengeButtons.Count; i++)
        {
            var button = this.challengeButtons[i];
            if (i < challenges.Count)
            {
                button.gameObject.SetActive(true);
                button.Setup(challenges[i]);
                button.SetSelected(challenges[i].buildingName == this.selectedBuilding);
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }
    }

}