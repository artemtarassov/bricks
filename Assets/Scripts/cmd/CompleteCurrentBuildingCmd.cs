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


        var progress = pd.GetBuildingProgressByName(currentBuildingName);
        var state = progress.State;     

        if (state == BuildingState.Locked)
        {
            throw new System.Exception($"CompleteCurrentBuildingCmd: current building {currentBuildingName} is locked");
        }

        progress.IncCompletedBuildingCounter();
        playerModel.SetCurrentBuilding(BuildingState.Completed);

        var currentBuilding = cityModel.GetBuildingByName(currentBuildingName);
        var allElements = currentBuilding.GetElements();
        foreach (var element in allElements)
        {
            element.gameObject.SetActive(true);
            element.EnableBricks(false);
            element.EnableVisuals(true);
        }
        //Debug.Log("CompleteCurrentBuildingCmd: completed building " + currentBuildingName + " elements " + allElements.Count);


        var allBuildingNames = BuildingNameUtil.GetAllBuildingNames();
        var currentBuildingIndex = allBuildingNames.IndexOf(currentBuildingName);
        var nextBuildingIndex = currentBuildingIndex + 1;
        if (nextBuildingIndex < allBuildingNames.Count)
        {
            var nextBuildingName = allBuildingNames[nextBuildingIndex];
            playerModel.playerData.GetBuildingProgressByName(nextBuildingName).SetState(BuildingState.Unlocked);
        }



        CamModel.Instance.MoveCameraToBuilding();
        ViewModel.Instance.Fade(FadeType.Flash);
        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);

        new ShowViewCmd(ViewName.CompleteView).Run();
    }

}