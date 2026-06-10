using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RestartCurrentGroupCmd
{
    public void Run()
    {
        var pd = PlayerModel.Instance.playerData;
        pd.currentElement = null;
        PlayerModel.Instance.SetCurrentGroup(GroupState.Unlocked);
        CityModel.Instance.DeactivateAllElements();
        new PlayCurrentGroupCmd().Run();
    }

}