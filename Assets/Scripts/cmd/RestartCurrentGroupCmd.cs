using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RestartCurrentGroupCmd
{
    public void Run()
    {
        var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
        progress.RemoveCurrentElement();
        progress.SetState(BuildingState.Unlocked);
        
        CityModel.Instance.DeactivateAllElements();
        new PlayCurrentBuildingCmd().Run();
    }

}