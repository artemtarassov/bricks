using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CompleteCurrentGroupCmd
{
    public void Run()
    {
        Debug.Log("CompleteCurrentGroupCmd Run");
        var cityModel = CityModel.Instance;
        var playerModel = PlayerModel.Instance;
        var pd = playerModel.playerData;
        var currentGroupName = pd.currentGroupName;

        var state = pd.progress.Find(p => p.groupName == currentGroupName)?.state;

        if (state == GroupState.Locked)
        {
            throw new System.Exception($"CompleteCurrentGroupCmd: current group {currentGroupName} is locked");
        }

        playerModel.SetCurrentGroup(GroupState.Completed);

        var currentGroup = cityModel.GetGroupByName(currentGroupName);
        var allElements = currentGroup.GetElements();
        foreach (var element in allElements)
        {
            element.gameObject.SetActive(true);
            element.EnableBricks(false);
            element.EnableVisuals(true);
        }
        Debug.Log("CompleteCurrentGroupCmd: completed group " + currentGroupName + " elements " + allElements.Count);


        CamModel.Instance.MoveCameraToElementGroup();
        ViewModel.Instance.Fade(FadeType.Flash);
        PlayerModel.Instance.OnPlayerDataChanged?.Invoke();
        ViewModel.Instance.ChangeBottomNav(BottomNav.MainNav);

        new ShowViewCmd(ViewName.CompleteView).Run();


        /*var nextGroupName = cityModel.GetNextGroupName();
        if (nextGroupName == null)
        {
            //out of groups, game completed.
            Debug.LogError("UnlockNextCmd no groups");
        }
        else
        {
            cityModel.SetCurrentGroupName(nextGroupName);
            currentElementData = null;
            currentGroupName = nextGroupName;
            playerModel.playerData.currentGroupName = nextGroupName;
            playerModel.playerData.currentElement = null;
            this.Run();
        }*/
    }

}