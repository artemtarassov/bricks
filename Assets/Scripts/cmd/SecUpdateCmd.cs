using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class SecUpdateCmd
{

    private int currentTimestamp;
    public SecUpdateCmd()
    {
        this.currentTimestamp = TimeUtils.GetUnixTimestamp();
    }
    public void Run()
    {
        PlayerModel.Instance.playerData.secondsPlaying += 1;
        PlayerModel.Instance.Save();
        IAPModel.Instance.Save();
        if (ViewModel.Instance.HasAnyView())
        {
            return;
        }
        var progress = PlayerModel.Instance.playerData.GetCurrentGroupProgress();
        if (progress.state == GroupState.Playing)
        {
            UpdateOutOfSpace();
            UpdateAdditionalEmitter();
        }
        UpdateDailyReward();
    }

    private void UpdateDailyReward()
    {
        var hasGoldenTicket = IAPModel.Instance.DidPurchaseComplete(IAPProductName.GoldenTicket);
        var hasGoldenTicketTemp = IAPModel.Instance.DidPurchaseComplete(IAPProductName.GoldenTicketTemp);
        if (!hasGoldenTicket && !hasGoldenTicketTemp)
        {
            return;
        }
        var timestamp = PlayerModel.Instance.playerData.lastDailyRewardTimestamp;
        var curTime = this.currentTimestamp;
        var timeSinceLastReward = curTime - timestamp;
        var secInDay = 12 * 60 * 60;
        if (timeSinceLastReward >= secInDay)
        {
            var coins = 0;
            if (hasGoldenTicket)
            {
                coins = RemoteConfigModel.Instance.RemoteConfig.DailyRewardCoinsGoldenTicket;
            }
            else if (hasGoldenTicketTemp)
            {
                coins = RemoteConfigModel.Instance.RemoteConfig.DailyRewardCoinsGoldenTicketTemp;
            }
            PlayerModel.Instance.AddDailyRewardCoins(coins);
            new ToastCmd("Daily reward coins: +" + coins).Run();
        }
    }

    private void UpdateOutOfSpace()
    {
        var isOut = ModelUtils.IsOutOfSpace();
        if (!isOut)
        {
            ViewModel.Instance.ResetOutOfSpaceCounter();
            return;
        }
        ViewModel.Instance.IncOutOfSpaceCounter(); 
        if (ViewModel.Instance.OutOfSpaceCounter == 3)
        {
            new ShowOutOfSpaceCmd().Run();
        }
    }

    private void UpdateAdditionalEmitter()
    {
        var playerData = PlayerModel.Instance.playerData;

        if (playerData.additionalEmitterUnlockTimeoutTimestamp <= 0)
        {
            return;
        }

        var curTimestamp = this.currentTimestamp;
        var timeoutReached = playerData.additionalEmitterUnlockTimeoutTimestamp <= curTimestamp;
        if (!timeoutReached)
        {
            return;
        }

        PlayerModel.Instance.LockAdditionalEmitter();

        if (SlotModel.Instance.Emitters[SlotModel.AdditionalEmitterIndex].IsEmpty)
        {
            SlotModel.Instance.LockAdditionalEmitter();
        }

    }
}