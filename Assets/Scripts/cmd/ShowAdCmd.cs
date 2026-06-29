using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShowAdCmd
{

    public void Run(RewardName rn, BuildingName buildingName = BuildingName.Undefined)
    {
        if (!AdModel.Instance.IsAdReady(rn))
        {
            new ToastCmd("No ads available").Run();
            return;
        }
        AudioListener.pause = true;
        AdModel.Instance.ShowAd(new AdRewardData(rn)
        {
            buildingName = buildingName
        });
        new ShowViewCmd(ViewName.LoadingView).Run();
    }

}