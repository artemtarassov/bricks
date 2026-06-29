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
        var currentBuildingName = progress.BuildingName;

        var currentElement = ModelUtils.GetCurrentElement();
        var index = CityModel.Instance.GetElementIndex(currentElement);
        CityModel.Instance.ActivateElements(index - 1);

        if (BuildingNameUtil.IsChallengeBuilding(currentBuildingName))
        {
            var firstBuilding = BuildingNameUtil.allBuildingNamesRegular[0];
            PlayerModel.Instance.SetCurrentBuilding(firstBuilding, BuildingState.Unlocked);
            CityModel.Instance.SetCurrentBuildingName(firstBuilding);
            new ShowViewCmd(ViewName.ChallengesView).Run();
        }
        else
        {
            PlayerModel.Instance.SetCurrentBuilding(BuildingState.Unlocked);
            CityModel.Instance.SetCurrentBuildingName(progress.BuildingName);
        }

        ViewModel.Instance.ResetOutOfSpaceCounter();
        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);
        ViewModel.Instance.ChangeTopNav(TopNav.None);

        ViewModel.Instance.Fade(FadeType.Flash);
        SlotModel.Instance.Clear();
        CamModel.Instance.MoveCameraToBuilding();
    }

}