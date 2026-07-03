using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CompleteCurrentBuildingCmd
{
    public void Run()
    {
        var cityModel = CityModel.Instance;
        var playerModel = PlayerModel.Instance;
        var pd = playerModel.playerData;
        var currentBuildingName = pd.CurrentBuildingName;

        playerModel.SetCurrentBuilding(BuildingState.Completed);

        var currentBuilding = cityModel.GetBuildingByName(currentBuildingName);
        var allElements = currentBuilding.GetElements();
        foreach (var element in allElements)
        {
            element.gameObject.SetActive(true);
            element.EnableBricks(false);
            element.EnableVisuals(true);
        }

        CamModel.Instance.MoveCameraToBuilding();
        ViewModel.Instance.Fade(FadeType.Flash);
        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);
        ViewModel.Instance.ChangeTopNav(TopNav.Coins);

        if (RemoteConfigModel.Instance.RemoteConfig.FillAttemptsAfterRestart)
        {
            var maxAttempts = RemoteConfigModel.Instance.RemoteConfig.MaxAttempts;
            playerModel.FillAttempts(currentBuildingName, maxAttempts, maxAttempts);
        }

        new ShowViewCmd(ViewName.CompleteView).Run();
    }

}