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
        this.selectedBuilding = BuildingNameUtil.allBuildingNamesChallenges[0];
        this.startBtn.GetComponent<HoldButton>().OnClick.AddListener(OnClickStart);
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
    }

    private void OnClickStart()
    {
        new HideViewCmd(ViewName.ChallengesView).Run();
        new SoundCmd(SoundModel.Instance.CLICK2).Run();
        new PlayCurrentBuildingCmd().Run(this.selectedBuilding);
    }

    public override void OnBackgroundTap()
    {
        new HideViewCmd(ViewName.ChallengesView).Run();
    }

    void Start()
    {
    }
    void OnDestroy()
    {
    }

    public override void OnHidden()
    {

    }

    public override void OnShown()
    {
        this.UpateButtons();
        this.UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (this.selectedBuilding == BuildingName.Undefined)
        {
            this.attemptsRow.gameObject.SetActive(false);
        }
        var progess = PlayerModel.Instance.playerData.GetBuildingProgressByName(this.selectedBuilding);
        this.attemptsRow.UpdateValues(progess.attempts);
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