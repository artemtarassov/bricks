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
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChange;
        ViewModel.Instance.OnShowView += OnViewUpdate;
        ViewModel.Instance.OnHideView += OnViewUpdate;
        PlayerModel.Instance.OnPlayerDataChanged += UpdateVisibility;
        UpdateVisibility();
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
    }

    private GroupState state => PlayerModel.Instance.playerData.GetCurrentGroupProgress().state;


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

        if (state == GroupState.Completed)
        {
            this.btnRestart.gameObject.SetActive(true);
            this.btnNext.gameObject.SetActive(true);
            UpdateLeftRightButtonVisibility();
            UpdateGroupTitle();
            AnimateIn();
            return;
        }

        if (state == GroupState.Unlocked)
        {
            var progress = PlayerModel.Instance.playerData.GetCurrentGroupProgress();
            if (progress.currentElement == null)
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
        var currentGroupName = PlayerModel.Instance.playerData.currentGroupName;
        var progressData = PlayerModel.Instance.playerData.GetCurrentGroupProgress();
        var completedElements = progressData.completedElementsCounter;
        var maxElements = CityModel.Instance.GetGroupByName(currentGroupName).GetElements().Count;
        //this.groupTitle.text = Loca.GetThemeName(currentGroupName);

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
        var currentGroup = PlayerModel.Instance.playerData.currentGroupName;
        var groupNames = PlayerModel.Instance.GetPlayableGroupNames();
        var currentIndex = groupNames.IndexOf(currentGroup);
        var hasPrevious = currentIndex > 0;
        var hasNext = currentIndex < groupNames.Count - 1;
        this.btnLeft.gameObject.SetActive(hasPrevious);
        this.btnRight.gameObject.SetActive(hasNext);
    }


    private void OnBtnRestartClicked()
    {
        //
        new SoundCmd(SoundModel.Instance.CLICK2).Run();
        new RestartCurrentGroupCmd().Run();
    }

    private void OnBtnContinueClicked()
    {
        //
        new SoundCmd(SoundModel.Instance.CLICK2).Run();
        new PlayCurrentGroupCmd().Run();
    }

    private void OnBtnStartClicked()
    {
        //
        new SoundCmd(SoundModel.Instance.CLICK2).Run();
        new PlayCurrentGroupCmd().Run();
    }

    private void OnBtnNextClicked()
    {
        //
        new SoundCmd(SoundModel.Instance.CLICK1).Run();
        new SwitchGroupCmd().Run(1);
    }

    private void OnBtnLeftClicked()
    {
        new SoundCmd(SoundModel.Instance.CLICK1).Run();
        new SwitchGroupCmd().Run(-1);
    }

    private void OnBtnRightClicked()
    {
        new SoundCmd(SoundModel.Instance.CLICK1).Run();
        new SwitchGroupCmd().Run(1);
    }



}