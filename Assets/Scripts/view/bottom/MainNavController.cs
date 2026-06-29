using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainNavController : MonoBehaviour
{
    [SerializeField] private GameObject content;

    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnNext;
    [SerializeField] private Button btnLeft;
    [SerializeField] private Button btnRight;

    [SerializeField] private Button btnPremium;
    [SerializeField] private Button btnClose;

    [SerializeField] private TMP_Text groupTitle;

    private Vector3 startPos;


    void Start()
    {
        this.startPos = this.content.transform.localPosition;
        btnRestart.GetComponent<HoldButton>().OnClick.AddListener(OnBtnRestartClicked);
        btnContinue.GetComponent<HoldButton>().OnClick.AddListener(OnBtnContinueClicked);
        btnStart.GetComponent<HoldButton>().OnClick.AddListener(OnBtnStartClicked);
        btnNext.GetComponent<HoldButton>().OnClick.AddListener(OnBtnNextClicked);
        btnLeft.GetComponent<HoldButton>().OnClick.AddListener(OnBtnLeftClicked);
        btnRight.GetComponent<HoldButton>().OnClick.AddListener(OnBtnRightClicked);
        btnPremium.GetComponent<BtnIAP>().onClicked = OnBtnPremiumClicked;
        btnClose.GetComponent<HoldButton>().OnClick.AddListener(OnCloseClicked);
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChange;
        ViewModel.Instance.OnShowView += OnViewUpdate;
        ViewModel.Instance.OnHideView += OnViewUpdate;
        PlayerModel.Instance.OnPlayerDataChanged += UpdateVisibility;
        UpdateVisibility();
    }

    private void OnCloseClicked()
    {
        new GoBackBtnCmd().Run();
    }

    private void OnBtnPremiumClicked(IAPProductName pn)
    {
        //
        new RequestPurchaseCmd(pn).Run();
    }

    private void OnViewUpdate(ViewName viewName)
    {
        UpdateVisibility();
    }

    private void OnBottomNavChange(BottomNav nav)
    {
        UpdateVisibility();
    }


    private void OnDestroy()
    {
        ViewModel.Instance.OnBottomNavChange -= OnBottomNavChange;
        PlayerModel.Instance.OnPlayerDataChanged -= UpdateVisibility;
        ViewModel.Instance.OnShowView -= OnViewUpdate;
        ViewModel.Instance.OnHideView -= OnViewUpdate;
    }

    private void DisableAllButtons()
    {
        btnRestart.gameObject.SetActive(false);
        btnContinue.gameObject.SetActive(false);
        btnStart.gameObject.SetActive(false);
        btnNext.gameObject.SetActive(false);
        btnLeft.gameObject.SetActive(false);
        btnRight.gameObject.SetActive(false);
        btnPremium.gameObject.SetActive(false);
        btnClose.gameObject.SetActive(false);

    }

    private BuildingProgressData progress => PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
    private BuildingState state => progress.State;
    private bool isChallenge => BuildingNameUtil.IsChallengeBuilding(progress.BuildingName);


    private void UpdateVisibility()
    {
        var nav = ViewModel.Instance.CurrentBottomNav;
        if (nav != BottomNav.MainNav)
        {
            this.content.SetActive(false);
            return;
        }
        var hasViews = ViewModel.Instance.HasAnyView();
        if (hasViews)
        {
            this.content.SetActive(false);
            return;
        }
        DisableAllButtons();

        if (state == BuildingState.Premium)
        {
            this.btnPremium.gameObject.SetActive(true);
            UpdateLeftRightButtonVisibility();
            UpdateGroupTitle();
            AnimateIn();
            return;
        }

        if (state == BuildingState.Completed)
        {
            this.btnRestart.gameObject.SetActive(true);
            this.btnNext.gameObject.SetActive(this.isChallenge == false);
            this.btnClose.gameObject.SetActive(this.isChallenge);
            UpdateLeftRightButtonVisibility();
            UpdateGroupTitle();
            AnimateIn();
            return;
        }

        if (state == BuildingState.Unlocked || state == BuildingState.Locked || state == BuildingState.Playing)
        {
            if (progress.GetCurrentElement() == null)
            {
                this.btnStart.gameObject.SetActive(true);
            }
            else
            {
                this.btnContinue.gameObject.SetActive(true);
            }
            UpdateLeftRightButtonVisibility();
            UpdateGroupTitle();
            AnimateIn();
            return;
        }
        this.content.SetActive(false);
    }

    private void UpdateGroupTitle()
    {
        var currentBuildingName = PlayerModel.Instance.playerData.CurrentBuildingName;
        var progressData = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        var completedElements = progressData.CompletedElementsCounter;
        var maxElements = CityModel.Instance.GetBuildingByName(currentBuildingName).GetElements().Count;
        var isChallenge = BuildingNameUtil.IsChallengeBuilding(currentBuildingName);

        if (isChallenge)
        {
            this.groupTitle.text = Loca.GetBuildingNameTranslation(currentBuildingName);
            return;
        }
        if (completedElements >= maxElements)
        {
            this.groupTitle.text = "Completed";
            return;
        }
        this.groupTitle.text = $"{completedElements}/{maxElements} completed";
    }

    private void AnimateIn()
    {
        if (this.content.activeSelf)
        {
            return;
        }
        this.content.SetActive(true);
        this.content.transform.localPosition = this.startPos - new Vector3(0, 500, 0);
        this.content.transform.DOLocalMove(this.startPos, Durations.NavTransition).SetEase(Ease.OutSine);
    }

    private void UpdateLeftRightButtonVisibility()
    {
        var currentBuildingName = PlayerModel.Instance.playerData.CurrentBuildingName;
        var names = BuildingNameUtil.GetAllBuildingNames(false);

        var currentIndex = names.IndexOf(currentBuildingName);
        var hasPrevious = currentIndex > 0;
        var hasNext = true;//currentIndex < names.Count - 1;

        var isChallenge = BuildingNameUtil.IsChallengeBuilding(currentBuildingName);
        if (isChallenge)
        {
            hasPrevious = false;
            hasNext = false;
        }


        //Debug.Log($"MainNavController UpdateLeftRightButtonVisibility: hasPrevious {hasPrevious} hasNext {hasNext}, currentIndex {currentIndex} unlockedBuildingNames count {unlockedBuildingNames.Count}");
        this.btnLeft.gameObject.SetActive(hasPrevious);
        this.btnRight.gameObject.SetActive(hasNext);
    }


    private void OnBtnRestartClicked()
    {
        //
        new SoundCmd(SoundModel.Instance.CLICK2).Run();
        new BtnCmd(progress.BuildingName).Run(BtnCmd.BtnAction.Restart);
    }

    private void OnBtnContinueClicked()
    {
        //
        new SoundCmd(SoundModel.Instance.CLICK2).Run();
        new BtnCmd(progress.BuildingName).Run(BtnCmd.BtnAction.Continue);
    }

    private void OnBtnStartClicked()
    {
        //
        new SoundCmd(SoundModel.Instance.CLICK2).Run();
        new BtnCmd(progress.BuildingName).Run(BtnCmd.BtnAction.Restart);
    }

    private void OnBtnNextClicked()
    {
        //
        new SoundCmd(SoundModel.Instance.CLICK1).Run();

        var currentBuildingName = PlayerModel.Instance.playerData.CurrentBuildingName;
        var names = BuildingNameUtil.GetAllBuildingNames(false);
        var isLast = names.Last() == currentBuildingName;

        if (isLast)
        {
            new ShowThankyouCmd().Run();
            return;
        }
        new SwitchBuildingCmd().Run(1);
    }

    private void OnBtnLeftClicked()
    {
        new SoundCmd(SoundModel.Instance.CLICK1).Run();
        new SwitchBuildingCmd().Run(-1);
    }

    private void OnBtnRightClicked()
    {
        OnBtnNextClicked();
    }



}