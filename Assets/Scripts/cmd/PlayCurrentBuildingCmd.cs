using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayCurrentBuildingCmd
{
    public void Run()
    {
        ViewModel.Instance.Fade(FadeType.Flash);
        var pd = PlayerModel.Instance.playerData;
        var buildingName = pd.GetCurrentBuildingProgress().BuildingName;
        var state = pd.GetCurrentBuildingProgress().State;

        if (state == BuildingState.Locked)
        {
            throw new System.Exception($"PlayCurrentGroupCmd: current group {buildingName} is locked");
        }

        if (state == BuildingState.Completed)
        {
            throw new System.Exception($"PlayCurrentGroupCmd: current group {buildingName} is already completed");
        }

        var group = CityModel.Instance.GetBuildingByName(buildingName);
        Assert.IsNotNull(group, $"PlayCurrentGroupCmd: failed to find group with name {buildingName}");


        var progress = pd.GetCurrentBuildingProgress();
        Assert.IsNotNull(progress, $"PlayCurrentGroupCmd: failed to find progress for current building {buildingName}");

        if (progress.GetCurrentElement() == null)
        {
            var elements = group.GetElements();
            var firstElement = elements.FirstOrDefault();
            Assert.IsNotNull(firstElement, $"PlayCurrentGroupCmd: failed to find first element in building {buildingName}");
            this.MoveCameraToCityElement(firstElement);
            Debug.Log("PlayCurrentGroupCmd: playing building " + buildingName + " first element " + firstElement.dataKey);
        }
        else
        {
            var currentElement = group.GetElements().ToList().Find(e => e.dataKey == progress.GetCurrentElement().dataKey);
            Assert.IsNotNull(currentElement, $"PlayCurrentGroupCmd: failed to find current element with dataKey {progress.GetCurrentElement().dataKey} in building {buildingName}");
            this.MoveCameraToCityElement(currentElement);
            Debug.Log("PlayCurrentGroupCmd: playing building " + buildingName + " current element " + currentElement.dataKey);
        }

        PlayerModel.Instance.SetCurrentBuilding(buildingName, BuildingState.Playing);
        ViewModel.Instance.ChangeBottomNav(BottomNav.Slots);
        new UnlockNextCmd().Run();
    }

    private void MoveCameraToCityElement(CityElement cityElement)
    {
        var camPos = cityElement.camPos;
        var camRot = cityElement.camRot;
        var cam = Camera.main;
        cam.transform.position = camPos;
        cam.transform.rotation = Quaternion.Euler(camRot);

        //move camera away from the direction its facing.
        /*var forward = cam.transform.forward;
        cam.transform.position += forward * -10f;
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y - 5f, cam.transform.position.z);

        cam.transform.LookAt(cityElement.GetAveragePosition());*/
    }

}