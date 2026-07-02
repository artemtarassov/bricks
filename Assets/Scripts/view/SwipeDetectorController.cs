using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using System.Linq;

[RequireComponent(typeof(Canvas))]
public class SwipeDetectorController : MonoBehaviour
{
    private SwipeDetector swipeDetector;
    void Start()
    {
        this.swipeDetector = this.gameObject.AddComponent<SwipeDetector>();
        this.swipeDetector.OnSwipeDetected = OnSwipeDetected;
        ViewModel.Instance.OnBottomNavChange += OnChangeBottomNav;
        this.OnChangeBottomNav(ViewModel.Instance.CurrentBottomNav);
    }

    private void OnSwipeDetected(SwipeDetector.SwipeDirection direction)
    {
        if (direction == SwipeDetector.SwipeDirection.Left)
            OnBtnNextClicked();
        else if (direction == SwipeDetector.SwipeDirection.Right)
            OnBtnLeftClicked();
    }

    private void OnChangeBottomNav(BottomNav nav)
    {
        var enable = nav == BottomNav.MainNav || nav == BottomNav.ThankYou;
        if (enable == this.swipeDetector.enabled)
        {
            return;
        }
        this.swipeDetector.enabled = enable;
    }


    void OnDestroy()
    {
        this.swipeDetector.OnSwipeDetected = null;
        ViewModel.Instance.OnBottomNavChange -= OnChangeBottomNav;
    }


    private void OnBtnNextClicked()
    {
        //
        new SwitchBuildingCmd().Run(1);
    }

    private void OnBtnLeftClicked()
    {
        new SwitchBuildingCmd().Run(-1);
    }

}