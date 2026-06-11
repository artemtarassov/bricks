using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SwitchGroupCmd
{
    public void Run(int direction = 0)//0=current group, 1=next group, -1=previous group
    {
        Debug.Log("SwitchGroupCmd Run");
        var cityModel = CityModel.Instance;
        var playerModel = PlayerModel.Instance;
        var pd = playerModel.playerData;
        var currentGroupName = pd.currentGroupName;

        var groupNames = playerModel.GetPlayableGroupNames();
        var currentGroupIndex = groupNames.FindIndex(g => g == currentGroupName);
        var nextGroupIndex = direction == 0 ? currentGroupIndex : direction == 1 ? currentGroupIndex + 1 : currentGroupIndex - 1;
        if (nextGroupIndex < 0)
        {
            nextGroupIndex = groupNames.Count - 1;
        }
        if (nextGroupIndex >= groupNames.Count)
        {
            nextGroupIndex = 0;
        }
        var nextGroupName = groupNames[nextGroupIndex];
        var progress = pd.progress.Find(p => p.groupName == nextGroupName);
        cityModel.SetCurrentGroupName(nextGroupName);


        playerModel.SetCurrentGroup(nextGroupName, progress.state);

        if (progress.currentElement != null)
        {
            var currentElement = cityModel.GetElementByDataKey(progress.currentElement.dataKey);
            var index = CityModel.Instance.GetElementIndex(currentElement);
            CityModel.Instance.ActivateElements(index - 1);
        }


        CamModel.Instance.MoveCameraToElementGroup();

        var allGroups = cityModel.GetAllGroupNames();
        for (var i = 0; i < allGroups.Count; i++)
        {
            var groupName = allGroups[i];
            var group = cityModel.GetGroupByName(groupName);
            group.gameObject.SetActive(groupName == nextGroupName);
        }

        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);
        ViewModel.Instance.Fade(FadeType.Flash);

        ChangeSkyMaterial(nextGroupName);
    }

    private void ChangeSkyMaterial(string nextGroupName)
    {
        var cm = ColoredMaterials.Instance;
        if (nextGroupName == "Preset_House_05")
        {
            RenderSettings.skybox = cm.GetMaterialByName("Sky 01");
        }
        else if (nextGroupName == "Tower_House")
        {
            RenderSettings.skybox = cm.GetMaterialByName("Sky 04");
        }
        else if (nextGroupName == "Ruins1_House")
        {
            RenderSettings.skybox = cm.GetMaterialByName("Sky 02");
        }
        else
            RenderSettings.skybox = cm.GetMaterialByName("Sky 01");
    }

}