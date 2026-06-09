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
        btnRestart.GetComponent<HoldButton>().OnFirstTouch.AddListener(OnBtnRestartClicked);
        btnContinue.GetComponent<HoldButton>().OnFirstTouch.AddListener(OnBtnContinueClicked);
        btnStart.GetComponent<HoldButton>().OnFirstTouch.AddListener(OnBtnStartClicked);
        btnNext.GetComponent<HoldButton>().OnFirstTouch.AddListener(OnBtnNextClicked);
        btnLeft.GetComponent<HoldButton>().OnFirstTouch.AddListener(OnBtnLeftClicked);
        btnRight.GetComponent<HoldButton>().OnFirstTouch.AddListener(OnBtnRightClicked);
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
        this.groupTitle.text = Loca.GetThemeName(currentGroupName);
    }

    private void AnimateIn()
    {
        if (this.content.activeSelf)
        {
            return;
        }
        this.content.SetActive(true);
        this.content.transform.localPosition = this.startPos - new Vector3(0, 500, 0);
        this.content.transform.DOLocalMove(this.startPos, 0.5f).SetEase(Ease.OutSine);
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
        new RestartCurrentGroupCmd().Run();
    }

    private void OnBtnContinueClicked()
    {
        //
        new PlayCurrentGroupCmd().Run();
    }

    private void OnBtnStartClicked()
    {
        //
        new PlayCurrentGroupCmd().Run();
    }

    private void OnBtnNextClicked()
    {
        //
        new SwitchGroupCmd().Run(1);
    }

    private void OnBtnLeftClicked()
    {
        new SwitchGroupCmd().Run(-1);
    }

    private void OnBtnRightClicked()
    {
        new SwitchGroupCmd().Run(1);
    }



}