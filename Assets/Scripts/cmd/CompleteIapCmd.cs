using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum IapResponse
{
    Undefined,
    Success,
    Restore,
    Failed,
}

public class CompleteIapCmd
{
    private string productId;
    private string payload;
    private string transactionId;

    public bool report = true;

    public CompleteIapCmd(string p, string payload = null, string transactionId = null)
    {
        this.productId = p;
        this.payload = payload;
        this.transactionId = transactionId;
    }

    public void Run(IapResponse response, bool report = true)
    {
        if (productId == IAPModel.PremiumBuilding1)
        {
            if (response == IapResponse.Success || response == IapResponse.Restore)
            {
                var progress = PlayerModel.Instance.playerData.GetBuildingProgressByName(BuildingName.Preset_Bath_House_01);
                progress.SetState(BuildingState.Unlocked);
                PlayerModel.Instance.OnPlayerDataChanged?.Invoke();
            }
        }

        if (productId == IAPModel.AdditionalSpace)
        {
            if (response == IapResponse.Success || response == IapResponse.Restore)
            {
                PlayerModel.Instance.UnlockAdditionalEmitter();
                SlotModel.Instance.UnlockAdditionalEmitter();
                ViewModel.Instance.ResetOutOfSpaceCounter();
            }
        }

        if (productId == IAPModel.GoldenTicket || productId == IAPModel.GoldenTicketTemp)
        {
            if (response == IapResponse.Success || response == IapResponse.Restore)
            {
                PlayerModel.Instance.UnlockAdditionalEmitter();
                SlotModel.Instance.UnlockAdditionalEmitter();
                ViewModel.Instance.ResetOutOfSpaceCounter();
            }

            if (response == IapResponse.Success)
            {
                var rc = RemoteConfigModel.Instance.RemoteConfig;
                if (productId == IAPModel.GoldenTicketTemp)
                {
                    PlayerModel.Instance.AddDailyRewardCoins(rc.DailyRewardCoinsGoldenTicketTemp);
                }
                if (productId == IAPModel.GoldenTicket)
                {
                    PlayerModel.Instance.AddDailyRewardCoins(rc.DailyRewardCoinsGoldenTicket);
                }
            }
        }

        if (response == IapResponse.Success || response == IapResponse.Restore)
        {
            IAPModel.Instance.TrySetPurchaseCompleted(
                productId,
                transactionId,
                response != IapResponse.Success
            );
        }

    }


}
