using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class CompleteAdRewardCmd
{
    public void Run(string unit, bool recorded)
    {
        new HideViewCmd(ViewName.LoadingView).Run();
        AudioListener.pause = false;
        //
        var rd = AdModel.Instance.GetRewardData(unit);
        AdModel.Instance.SetRewardEarned(unit);

        Debug.Log("CompleteAdRewardCmd. unit: " + unit + ", recorded: " + recorded);

        if (rd.rewardName == RewardName.INTERSTITIAL)
        {
            var curTimestamp = TimeUtils.GetUnixTimestamp();
            var lastTriggerTimestamp = ViewModel.Instance.GoldenTicketViewTriggerTimestamp;

            var secDiff = curTimestamp - lastTriggerTimestamp;
            if (secDiff > 60 * 60)
            {
                var hasIapPrices = IAPModel.Instance.HasPriceForProduct(IAPModel.GoldenTicket) && IAPModel.Instance.HasPriceForProduct(IAPModel.GoldenTicketTemp);
                if (hasIapPrices)
                {
                    ViewModel.Instance.GoldenTicketViewTriggerTimestamp = curTimestamp;
                    new ShowViewCmd(ViewName.GoldenTicketView).Run();
                }
            }
            return;
        }

        if (!recorded)
        {
            return;
        }

        if (rd.rewardName == RewardName.SPACE1)
        {
            var index = SlotModel.Instance.GetLockedEmitterIndex();
            Assert.IsTrue(index != -1, "No locked emitter found to unlock");
            var curTimestamp = TimeUtils.GetUnixTimestamp();
            var additionalEmitterSec = RemoteConfigModel.Instance.RemoteConfig.AdditionalEmitterSec;
            PlayerModel.Instance.UnlockAdditionalEmitter(curTimestamp + additionalEmitterSec);
            SlotModel.Instance.UnlockAdditionalEmitter();
            ViewModel.Instance.OutOfSpaceSeconds = 0;
        }

        if (rd.rewardName == RewardName.ADD_ATTEMPT)
        {
            var maxAttempts = RemoteConfigModel.Instance.RemoteConfig.MaxAttempts;
            PlayerModel.Instance.FillAttempts(1, maxAttempts);
        }


    }

}

[System.Serializable]
sealed class CompeleteAdRewardCmd
{
    public string unit;
    public bool recorded;
}