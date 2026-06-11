using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayCurrentGroupCmd
{
    public void Run()
    {
        ViewModel.Instance.Fade(FadeType.Flash);
        var pd = PlayerModel.Instance.playerData;
        var groupName = pd.currentGroupName;

        var state = pd.progress.Find(p => p.groupName == groupName)?.state;

        if (state == GroupState.Locked)
        {
            throw new System.Exception($"PlayCurrentGroupCmd: current group {groupName} is locked");
        }

        if (state == GroupState.Completed)
        {
            throw new System.Exception($"PlayCurrentGroupCmd: current group {groupName} is already completed");
        }

        var group = CityModel.Instance.GetGroupByName(groupName);

        var progress = pd.GetCurrentGroupProgress();
        if (progress.currentElement == null)
        {
            var elements = group.GetElements();
            var firstElement = elements.FirstOrDefault();
            this.MoveCameraToCityElement(firstElement);
            Debug.Log("PlayCurrentGroupCmd: playing group " + groupName + " first element " + firstElement.dataKey);
        }
        else
        {
            var currentElement = group.GetElements().ToList().Find(e => e.dataKey == progress.currentElement.dataKey);
            this.MoveCameraToCityElement(currentElement);
            Debug.Log("PlayCurrentGroupCmd: playing group " + groupName + " current element " + currentElement.dataKey);
        }

        PlayerModel.Instance.SetCurrentGroup(groupName, GroupState.Playing);
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