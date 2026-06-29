using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UpdateSkyMaterialCmd
{
    public void Run()
    {
        this.ChangeSkyMaterial(PlayerModel.Instance.playerData.CurrentBuildingName);
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
            RenderSettings.skybox = cm.GetMaterialByName("Sky_challenges");
    }

}