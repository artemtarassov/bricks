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
            //UpdateNextCityElement();
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


    /*private void UpdateNextCityElement()
    {
        var currentElement = ModelUtils.GetCurrentElement();
        Assert.IsNotNull(currentElement, "Current city element is null in UpdateNextCityElement");
        var da = currentElement.dataContainer;
        Assert.IsNotNull(da, "Current city element data container is null");

        //Debug.Log($"SecUpdateCmd: UpdateNextCityElement: element={currentElement.name}, emittingBricks={da.ElementCountEmittingBricks()}, coloredBricks={da.ElementCountColoredBricks()}, allSlotsEmpty={da.AllSlotsEmpty()}");
        if (da.ElementCompleted() && da.AllSlotsEmpty() && currentElement.HasVisuals())
        {
            var currentGroup = CityModel.Instance.GetGroupByName(PlayerModel.Instance.playerData.currentGroupName);
            var groupCompleted = currentGroup.GetElements().All(e => e.dataContainer.ElementCompleted());
            if (groupCompleted)
            {
                new CompleteCurrentGroupCmd().Run();
            }
        }
    }*/


    private void UpdateOutOfSpace()
    {
        var cntEmitterSpace = SlotModel.Instance.CountEmptyEmitters();
        if (cntEmitterSpace > 0)
        {
            ViewModel.Instance.OutOfSpaceSeconds = 0;
            return;
        }
        var currentElement = ModelUtils.GetCurrentElement();
        Assert.IsNotNull(currentElement, "Current city element is null in UpdateOutOfSpace");
        var hasEmittingBricks = currentElement.dataContainer.ElementCountEmittingBricks() > 0;
        Debug.Log($"SecUpdateCmd: UpdateOutOfSpace: cntEmitterSpace={cntEmitterSpace}, hasEmittingBricks={hasEmittingBricks}");
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
            new ShowViewCmd(ViewName.OutOfSpaceView).Run();
            ViewModel.Instance.OutOfSpaceSeconds = int.MinValue;
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