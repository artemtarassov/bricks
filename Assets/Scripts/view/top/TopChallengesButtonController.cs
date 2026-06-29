using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class TopChallengesButtonController : MonoBehaviour
{
    [SerializeField] private GameObject challengesButton;

    void Start()
    {
        ViewModel.Instance.OnTopNavChange += OnTopNavChanged;
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChanged;
        ViewModel.Instance.OnHideView += OnHideView;
        ViewModel.Instance.OnShowView += OnShowView;
        this.challengesButton.GetComponent<HoldButton>().OnClick.AddListener(OnChallengesButtonClicked);
        this.UpdateVisibility();
    }

    private void OnHideView(ViewName vn)
    {
        UpdateVisibility();
    }

    private void OnShowView(ViewName vn)
    {
        UpdateVisibility();
    }

    void OnDestroy()
    {
        ViewModel.Instance.OnTopNavChange -= OnTopNavChanged;
        ViewModel.Instance.OnBottomNavChange -= OnBottomNavChanged;
        ViewModel.Instance.OnHideView -= OnHideView;
        ViewModel.Instance.OnShowView -= OnShowView;
    }

    private void OnTopNavChanged(TopNav topNav)
    {
        UpdateVisibility();
    }

    private void OnBottomNavChanged(BottomNav bottomNav)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        var topNav = ViewModel.Instance.CurrentTopNav;
        var bottomNav = ViewModel.Instance.CurrentBottomNav;

        var lastUnlockedChallenge = ChallengeModel.Instance.GetLastUnlockedChallenge();
        var hasView = ViewModel.Instance.HasAnyView();
        this.challengesButton.gameObject.SetActive(hasView == false && topNav != TopNav.Clock && bottomNav == BottomNav.MainNav && lastUnlockedChallenge != BuildingName.Undefined);
        if (this.challengesButton.gameObject.activeSelf)
        {
            var icon = this.challengesButton.GetComponentInChildren<ChallengeIcon>();
            icon.Setup(lastUnlockedChallenge);
        }
    }

    private void OnChallengesButtonClicked()
    {
        new ShowViewCmd(ViewName.ChallengesView).Run();
    }



}