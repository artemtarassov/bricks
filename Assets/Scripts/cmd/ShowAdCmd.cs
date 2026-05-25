using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShowAdCmd
{
    public void Run(RewardName rn)
    {
        if (!AdModel.Instance.AllReady())
        {
            new ToastCmd("no ads").Run();
            return;
        }
        AdModel.Instance.ShowAd(new AdRewardData(rn));
        new ShowViewCmd().Run(ViewName.LoadingView);
    }

}