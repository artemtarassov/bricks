using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class SecUpdateCmd
{

    private int currentTimestamp;
    private PlayerModel playerModel => PlayerModel.Instance;
    private ViewModel viewModel => ViewModel.Instance;
    private ChallengeModel challengeModel => ChallengeModel.Instance;
    private BuildingProgressData progress;
    private CityElementDataContainer currentElementData => progress.GetCurrentElement();
    public SecUpdateCmd()
    {
        this.currentTimestamp = TimeUtils.GetUnixTimestamp();
        this.progress = playerModel.playerData.GetCurrentBuildingProgress();
    }
    public void Run()
    {
        this.playerModel.playerData.secondsPlaying += 1;
        this.playerModel.Save();
        this.challengeModel.Save();
        FilePrefs.Save();

        if (viewModel.HasAnyView())
        {
            return;
        }
        if (progress.State == BuildingState.Playing)
        {
            UpdateGameOver();
            UpdateAdditionalEmitter();
        }
        UpdateDailyReward();
        UpdateAdLoading();
    }

    private void UpdateAdLoading()
    {
        var pm = playerModel.playerData;
        var hasGoldenTicket = IAPModel.Instance.DidPurchaseComplete(IAPProductName.GoldenTicket);
        var hasGoldenTicketTemp = IAPModel.Instance.HasTempGoldenTicket();

        if (hasGoldenTicket || hasGoldenTicketTemp)
        {
            AdModel.Instance.shouldLoadBanner = false;
            AdModel.Instance.shouldLoadInterstitial = false;
            return;
        }

        var secSinceInstall = TimeUtils.GetUnixTimestamp() - pm.installTimestamp;

        if (secSinceInstall >= RemoteConfigModel.Instance.RemoteConfig.ShowMidSessionAdAfterSec)
        {
            AdModel.Instance.shouldLoadInterstitial = true;
        }

        if (secSinceInstall >= RemoteConfigModel.Instance.RemoteConfig.ShowBannerAfterSec)
        {
            AdModel.Instance.shouldLoadBanner = true;
        }

    }

    private void UpdateDailyReward()
    {
        var hasGoldenTicket = IAPModel.Instance.DidPurchaseComplete(IAPProductName.GoldenTicket);
        var hasGoldenTicketTemp = IAPModel.Instance.HasTempGoldenTicket();
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

    private void UpdateGameOver()
    {
        var reason = ModelUtils.IsGameOver();
        if (reason == GameOverReason.Undefined)
        {
            viewModel.ResetOutOfSpaceCounter();
            playerModel.AddTimeoutSeconds(-1);
            return;
        }
        if (reason == GameOverReason.OutOfSpace)
        {
            viewModel.IncOutOfSpaceCounter();
            if (viewModel.OutOfSpaceCounter == 3)
            {
                new ShowGameOverCmd().Run(reason);
            }
            return;
        }
        if (reason == GameOverReason.OutOfTime)
        {
            new ShowGameOverCmd().Run(reason);
            return;
        }
    }

    private void UpdateAdditionalEmitter()
    {
        var playerData = playerModel.playerData;

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