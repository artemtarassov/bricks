using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SwitchBuildingCmd
{
    public void Run(int direction = 0)//0=current group, 1=next group, -1=previous group
    {
        //Debug.Log("SwitchBuildingCmd Run direction: " + direction);
        var cityModel = CityModel.Instance;
        var playerModel = PlayerModel.Instance;
        var pd = playerModel.playerData;
        var currentBuildingName = pd.CurrentBuildingName;
        var buildingNames = BuildingNameUtil.GetAllBuildingNames(false);
        var currentBuildingIndex = buildingNames.FindIndex(g => g == currentBuildingName);
        var nextBuildingIndex = direction == 0 ? currentBuildingIndex : direction == 1 ? currentBuildingIndex + 1 : currentBuildingIndex - 1;
        if (nextBuildingIndex < 0)
        {
            return;
        }
        if (nextBuildingIndex >= buildingNames.Count)
        {
            new ShowThankyouCmd().Run();
            return;
        }
        if (direction != 0)
        {
            new SoundCmd(SoundModel.Instance.CLICK1).Run();
        }
        var nextBuildingName = buildingNames[nextBuildingIndex];
        Assert.IsFalse(nextBuildingName == BuildingName.Undefined, "SwitchBuildingCmd: nextBuildingName is undefined");
        this.Run(nextBuildingName);
    }

    public void Run(BuildingName nextBuildingName)
    {
        var cityModel = CityModel.Instance;
        var playerModel = PlayerModel.Instance;
        var pd = playerModel.playerData;
        var progress = pd.GetBuildingProgressByName(nextBuildingName);
        Assert.IsNotNull(progress, $"SwitchBuildingCmd: progress is null for building {nextBuildingName}");
        cityModel.SetCurrentBuildingName(nextBuildingName);
        playerModel.SetCurrentBuilding(nextBuildingName, progress.State);

        if (progress.GetCurrentElement() != null)
        {
            var currentElement = cityModel.GetElementByDataKey(progress.GetCurrentElement().dataKey);
            var index = cityModel.GetElementIndex(currentElement);
            cityModel.ActivateElements(index - 1);
        }

        CamModel.Instance.MoveCameraToBuilding();
        ViewModel.Instance.ChangeTopNav(TopNav.None);
        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);
        //ViewModel.Instance.Fade(FadeType.Flash);
        new UpdateSkyMaterialCmd().Run();
    }


}