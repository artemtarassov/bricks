using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShowThankyouCmd
{
    public void Run()
    {
        CamModel.Instance.MoveCameraToSky();
        ViewModel.Instance.ChangeBottomNav(BottomNav.ThankYou);
        ViewModel.Instance.ChangeTopNav(TopNav.None);
        ViewModel.Instance.Fade(FadeType.Flash);
    }

}