using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GoBackBtnCmd
{
    public void Run()
    {
        new HideViewCmd().Run();

        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        if (progress.State == BuildingState.Playing || progress.State == BuildingState.Completed)
        {
            //ok
        }
        else
        {
            return;
        }

        PlayerModel.Instance.SetCurrentBuilding(BuildingState.Unlocked);
        var currentElement = ModelUtils.GetCurrentElement();
        var index = CityModel.Instance.GetElementIndex(currentElement);
        CityModel.Instance.ActivateElements(index - 1);
        ViewModel.Instance.ResetOutOfSpaceCounter();
        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);

        ViewModel.Instance.Fade(FadeType.Flash);
        SlotModel.Instance.Clear();
        CamModel.Instance.MoveCameraToBuilding();
    }

}