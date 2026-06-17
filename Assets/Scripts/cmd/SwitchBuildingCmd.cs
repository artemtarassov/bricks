using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SwitchBuildingCmd
{
    public void Run(int direction = 0)//0=current group, 1=next group, -1=previous group
    {
        Debug.Log("SwitchBuildingCmd Run direction: " + direction);
        var cityModel = CityModel.Instance;
        var playerModel = PlayerModel.Instance;
        var pd = playerModel.playerData;
        var currentBuildingName = pd.CurrentBuildingName;

        var buildingNames = playerModel.GetUnlockedBuildings();
        var currentGroupIndex = buildingNames.FindIndex(g => g == currentBuildingName);
        var nextGroupIndex = direction == 0 ? currentGroupIndex : direction == 1 ? currentGroupIndex + 1 : currentGroupIndex - 1;
        if (nextGroupIndex < 0)
        {
            nextGroupIndex = buildingNames.Count - 1;
        }
        if (nextGroupIndex >= buildingNames.Count)
        {
            nextGroupIndex = 0;
        }
        var nextGroupName = buildingNames[nextGroupIndex];
        Assert.IsFalse(nextGroupName == BuildingName.Undefined, "SwitchGroupCmd: nextGroupName is undefined");
        Debug.Log($"SwitchGroupCmd: current group {currentBuildingName} next group {nextGroupName}");
        var progress = pd.GetBuildingProgressByName(nextGroupName);
        Assert.IsNotNull(progress, $"SwitchGroupCmd: progress is null for group {nextGroupName}");
        cityModel.SetCurrentBuildingName(nextGroupName);
        playerModel.SetCurrentBuilding(nextGroupName, progress.State);

        if (progress.GetCurrentElement() != null)
        {
            var currentElement = cityModel.GetElementByDataKey(progress.GetCurrentElement().dataKey);
            var index = cityModel.GetElementIndex(currentElement);
            cityModel.ActivateElements(index - 1);
        }


        CamModel.Instance.MoveCameraToBuilding();

        for (var i = 0; i < buildingNames.Count; i++)
        {
            var buildingName = buildingNames[i];
            var building = cityModel.GetBuildingByName(buildingName);
            building.gameObject.SetActive(buildingName == nextGroupName);
        }

        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);
        ViewModel.Instance.Fade(FadeType.Flash);

        ChangeSkyMaterial(nextGroupName);
    }

    private void ChangeSkyMaterial(BuildingName nextBuildingName)
    {
        var cm = ColoredMaterials.Instance;
        if (cm == null)
        {
            return;
        }
        if (nextBuildingName == BuildingName.Preset_House_05)
        {
            RenderSettings.skybox = cm.GetMaterialByName("Sky 01");
        }
        else if (nextBuildingName == BuildingName.Tower_House)
        {
            RenderSettings.skybox = cm.GetMaterialByName("Sky 04");
        }
        else if (nextBuildingName == BuildingName.Ruins1_House)
        {
            RenderSettings.skybox = cm.GetMaterialByName("Sky 03");
        }
        else
            RenderSettings.skybox = cm.GetMaterialByName("Sky 01");
    }

}