using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GoBackBtnCmd
{
    public void Run()
    {
        new HideViewCmd().Run();
        
        PlayerModel.Instance.SetCurrentGroup(GroupState.Unlocked);
        var currentElement = ModelUtils.GetCurrentElement();
        var index = CityModel.Instance.GetElementIndex(currentElement);
        CityModel.Instance.ActivateElements(index - 1);
        ViewModel.Instance.OutOfSpaceSeconds = 0;
        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);

        ViewModel.Instance.Fade(FadeType.Flash);
        SlotModel.Instance.Clear();
        CamModel.Instance.MoveCameraToElementGroup();
    }

}