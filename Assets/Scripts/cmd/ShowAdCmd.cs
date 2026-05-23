using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShowAdCmd
{
    public void Run()
    {
        AdModel.Instance.ShowAd(new AdRewardData(RewardName.CASH1));
    }

}