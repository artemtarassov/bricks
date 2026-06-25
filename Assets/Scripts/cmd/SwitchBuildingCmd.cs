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

        var buildingNames = BuildingNameUtil.GetAllBuildingNames();
        var currentGroupIndex = buildingNames.FindIndex(g => g == currentBuildingName);
        var nextBuildingIndex = direction == 0 ? currentGroupIndex : direction == 1 ? currentGroupIndex + 1 : currentGroupIndex - 1;
        if (nextBuildingIndex < 0)
        {
            nextBuildingIndex = buildingNames.Count - 1;
        }
        if (nextBuildingIndex >= buildingNames.Count)
        {
            nextBuildingIndex = 0;
        }
        var nextBuildingName = buildingNames[nextBuildingIndex];
        Assert.IsFalse(nextBuildingName == BuildingName.Undefined, "SwitchBuildingCmd: nextBuildingName is undefined");
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

        /*var allBuildingNames = BuildingNameUtil.GetAllBuildingNames();
        for (var i = 0; i < allBuildingNames.Count; i++)
        {
            var buildingName = allBuildingNames[i];
            var building = cityModel.GetBuildingByName(buildingName);
            building.gameObject.SetActive(buildingName == nextBuildingName);
        }*/

        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);
        ViewModel.Instance.Fade(FadeType.Flash);

        /*
                RenderSettings.skybox = null;
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.red;
        */
        ChangeSkyMaterial(nextBuildingName);
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