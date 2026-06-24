using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ThankyouController : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private Button reviewButton;

    [SerializeField] private Button backButton;

    private Vector3 startPos;

    private float currentPercent;
    void Start()
    {
        this.startPos = this.content.transform.localPosition;
        this.content.SetActive(false);
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChange;

        this.backButton.GetComponent<HoldButton>().OnClick.AddListener(OnBackButtonClick);
        this.reviewButton.GetComponent<HoldButton>().OnClick.AddListener(OnReviewButtonClick);
    }

    private void OnReviewButtonClick()
    {
        new SoundCmd(SoundModel.Instance.CLICK1).Run();
        new RateAppCmd().Run();
    }

    private void OnBackButtonClick()
    {
        new SoundCmd(SoundModel.Instance.CLICK1).Run();
        new SwitchBuildingCmd().Run();
    }

    private void OnBottomNavChange(BottomNav nav)
    {
        UpdateVisibility();
    }

    void OnDestroy()
    {
        ViewModel.Instance.OnBottomNavChange -= OnBottomNavChange;
    }

    private void UpdateVisibility()
    {
        var nav = ViewModel.Instance.CurrentBottomNav;
        if (nav == BottomNav.ThankYou)
        {
            this.AnimateIn();
        }
        else
        {
            this.content.SetActive(false);
        }
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



}