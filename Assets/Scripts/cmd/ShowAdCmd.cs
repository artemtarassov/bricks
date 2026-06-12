using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShowAdCmd
{
    public void Run(RewardName rn)
    {
        if (!AdModel.Instance.IsAdReady(rn))
        {
            new ToastCmd("no ads").Run();
            return;
        }
        AudioListener.pause = true;
        AdModel.Instance.ShowAd(new AdRewardData(rn));
        new ShowViewCmd(ViewName.LoadingView).Run();
    }

}