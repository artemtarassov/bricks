using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class CompleteAdRewardCmd
{
    public void Run(string unit, bool recorded)
    {
        AdModel.Instance.SetRewardEarned(unit);
        new HideViewCmd().Run(ViewName.LoadingView);

        if (!recorded)
        {
            return;
        }

        if (unit == IAPModel.AdditionalSpace)
        {
            var index = SlotModel.Instance.UnlockNextEmitter();
            var playerData = PlayerModel.Instance.playerData;
            playerData.emitterUnlockTimestamp = TimeUtils.GetUnixTimestamp();
            playerData.emitterIndex = index;
        }
    }

}

[System.Serializable]
sealed class CompeleteAdRewardCmd
{
    public string unit;
    public bool recorded;
}