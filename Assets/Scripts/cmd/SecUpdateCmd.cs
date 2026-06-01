using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class SecUpdateCmd
{
    private CityElement currentElement;
    private int currentTimestamp;
    public SecUpdateCmd()
    {
        this.currentElement = CityModel.Instance.GetCurrentElement();
        this.currentTimestamp = TimeUtils.GetUnixTimestamp();
        Assert.IsNotNull(this.currentElement, "current element in cityModel not set");
    }
    public void Run()
    {
        PlayerModel.Instance.Save();
        IAPModel.Instance.Save();
        if (ViewModel.Instance.HasAnyView())
        {
            return;
        }
        UpdateOutOfSpace();
        UpdateAdditionalEmitter();
        UpdateNextCityElement();
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


    private void UpdateNextCityElement()
    {
        var da = currentElement.dataContainer;
        Assert.IsNotNull(da, "Current city element data container is null");

        //Debug.Log($"SecUpdateCmd: UpdateNextCityElement: element={currentElement.name}, emittingBricks={da.ElementCountEmittingBricks()}, coloredBricks={da.ElementCountColoredBricks()}, allSlotsEmpty={da.AllSlotsEmpty()}");
        if (da.ElementCompleted() && da.AllSlotsEmpty())
        {
            DOVirtual.DelayedCall(1, new UnlockNextCmd().Run, false);
        }
    }


    private void UpdateOutOfSpace()
    {
        var cntEmitterSpace = SlotModel.Instance.CountEmptyEmitters();
        if (cntEmitterSpace > 0)
        {
            ViewModel.Instance.OutOfSpaceSeconds = 0;
            return;
        }

        var hasEmittingBricks = currentElement.dataContainer.ElementCountEmittingBricks() > 0;
        if (hasEmittingBricks)
        {
            ViewModel.Instance.OutOfSpaceSeconds = 0;
            return;
        }
        /*var colorsInEmitters = SlotModel.Instance.Emitters.FindAll(e => e.HasColoredBricks).Select(e => e.brickData.color).ToHashSet();
        var colorsInCityElement = currentElement.GetBrickColors();

        foreach (var c in colorsInEmitters)
        {
            if (colorsInCityElement.Contains(c))
            {
                Debug.Log($"SecUpdateCmd color {c} is still present in emitters, skipping");
                return;
            }
        }*/

        ViewModel.Instance.OutOfSpaceSeconds++;
        if (ViewModel.Instance.OutOfSpaceSeconds == 3)
        {
            new ShowViewCmd().Run(ViewName.OutOfSpaceView);
            ViewModel.Instance.OutOfSpaceSeconds = 0;
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